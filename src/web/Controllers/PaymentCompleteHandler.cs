using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data.Business;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Security;
using OxxCommerceStarterKit.Core;
using System.Security.Principal;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using OxxCommerceStarterKit.Web.Services.Email;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class PaymentCompleteHandler : IPaymentCompleteHandler
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly IEmailService _emailService;
        private readonly ICustomerFactory _customerFactory;
        private readonly IOrderRepository _orderRepository;

        public PaymentCompleteHandler(IEmailService emailService, ICustomerFactory customerFactory, IOrderRepository orderRepository)
        {
            _emailService = emailService;
            _customerFactory = customerFactory;
            _orderRepository = orderRepository;
        }

        public void OnPaymentComplete(PurchaseOrderModel orderModel, IIdentity identity)
        {
            var order = _orderRepository.GetOrderByTrackingNumber(orderModel.TrackingNumber);
            // Create customer if anonymous
            CreateUpdateCustomer(order, identity);
            
            var shipment = order.OrderForms.First().Shipments.First();

            OrderStatusManager.ReleaseOrderShipment(shipment);
            OrderStatusManager.PickForPackingOrderShipment(shipment);

            order.AcceptChanges();

            // Send Email receipt 
            bool sendOrderReceiptResult = SendOrderReceipt(orderModel);
            Log.Debug("Sending receipt e-mail - " + (sendOrderReceiptResult ? "success" : "failed"));

            try
            {
                // Not extremely important that this succeeds. 
                // Stocks are continually adjusted from ERP.
                AdjustStocks(order); 
            }
            catch (Exception e)
            {
                Log.Error("Error adjusting inventory after purchase.", e);
            }

            ForwardOrderToErp(order);
        }

        public bool SendOrderReceipt(PurchaseOrderModel order)
        {
            return _emailService.SendOrderReceipt(order);
        }

        public void ForwardOrderToErp(PurchaseOrder purchaseOrder)
        {
            // TODO: Implement for your solution
        }

        public void AdjustStocks(PurchaseOrder order)
        {
            var warehouseRepository = ServiceLocator.Current.GetInstance<IWarehouseRepository>();
            var warehousesCache = warehouseRepository.List();
            var warehouseInventory = ServiceLocator.Current.GetInstance<IWarehouseInventoryService>();

            var expirationCandidates = new HashSet<ProductContent>();

            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            // Adjust inventory
            foreach (OrderForm f in order.OrderForms)
            {
                foreach (LineItem i in f.LineItems)
                {
                    try
                    {
                        var warehouse = warehousesCache.Where(w => w.Code == i.WarehouseCode).First();
                        var catalogEntry = CatalogContext.Current.GetCatalogEntry(i.CatalogEntryId);
                        var catalogKey = new CatalogKey(catalogEntry);
                        var inventory = new WarehouseInventory(warehouseInventory.Get(catalogKey, warehouse));

                        //inventory.ReservedQuantity += i.Quantity; 
                        if ((inventory.InStockQuantity -= i.Quantity) <= 0)
                        {
                            var contentLink = referenceConverter.GetContentLink(i.CatalogEntryId);
                            var variant = contentRepository.Get<VariationContent>(contentLink);

                            expirationCandidates.Add((ProductContent)variant.GetParent());
                        }

                        warehouseInventory.Save(inventory);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Unable to adjust inventory.", ex);
                    }

                }
            }

            // TODO: Determine if you want to unpublish products with no sellable variants
            // ExpireProductsWithNoInventory(expirationCandidates, contentRepository);
            // Alterntive approach is to notify the commerce admin about the products without inventory

        }

        public void CreateUpdateCustomer(PurchaseOrder order, IIdentity identity)
        {
            // try catch so this does not interrupt the order process.
            try
            {
                var billingAddress = order.OrderAddresses.FirstOrDefault(x => x.Name == Constants.Order.BillingAddressName);
                var shippingAddress = order.OrderAddresses.FirstOrDefault(x => x.Name == Constants.Order.ShippingAddressName);

                // create customer if anonymous, or update join customer club and selected values on existing user
                MembershipUser user = null;
                if (!identity.IsAuthenticated)
                {
                    string email = billingAddress.Email.Trim();

                    user = Membership.GetUser(email);
                    if (user == null)
                    {
                        var customer = CreateCustomer(email, Guid.NewGuid().ToString(), billingAddress.DaytimePhoneNumber, billingAddress, shippingAddress, false, createStatus =>
                        {
                            Log.Error("Failed to create user during order completion. " + createStatus.ToString());
                        });
                        if (customer != null)
                        {
                            order.CustomerId = Guid.Parse(customer.PrimaryKeyId.Value.ToString());
                            order.CustomerName = customer.FirstName + " " + customer.LastName;
                            order.AcceptChanges();

                            SetExtraCustomerProperties(order, customer);

                            RegisterPageController.SendWelcomeEmail(billingAddress.Email);
                        }
                    }
                    else
                    {
                        var customer = CustomerContext.Current.GetContactForUser(user);
                        order.CustomerName = customer.FirstName + " " + customer.LastName;
                        order.CustomerId = Guid.Parse(customer.PrimaryKeyId.Value.ToString());
                        order.AcceptChanges();
                        SetExtraCustomerProperties(order, customer);

                    }
                }
                else
                {
                    user = Membership.GetUser(identity.Name);
                    var customer = CustomerContext.Current.GetContactForUser(user);
                    SetExtraCustomerProperties(order, customer);
                }
            }
            catch (Exception ex)
            {
                // Log here
                Log.Error("Error during creating / update user", ex);
            }
        }

        protected CustomerContact CreateCustomer(string email, string password, string phone, OrderAddress billingAddress, OrderAddress shippingAddress, bool hasPassword, Action<MembershipCreateStatus> userCreationFailed)
        {
            return _customerFactory.CreateCustomer(email, password, phone, billingAddress, shippingAddress, hasPassword,
                userCreationFailed);
        }

        /// <summary>
        /// If customer has joined the members club, then add the interest areas to the
        /// customer profile.
        /// </summary>
        /// <remarks>
        /// The request to join the member club is stored on the order during checkout.
        /// </remarks>
        /// <param name="order">The order.</param>
        /// <param name="customer">The customer.</param>
        private void SetExtraCustomerProperties(PurchaseOrder order, CustomerContact customer)
        {
            // TODO: Refactor for readability
            // member club
            if (order.OrderForms[0][Constants.Metadata.OrderForm.CustomerClub] != null && ((bool)order.OrderForms[0][Constants.Metadata.OrderForm.CustomerClub]) == true)
            {
                customer.CustomerGroup = Constants.CustomerGroup.CustomerClub;

                // categories
                if (!string.IsNullOrEmpty(order.OrderForms[0][Constants.Metadata.OrderForm.SelectedCategories] as string))
                {
                    var s = (order.OrderForms[0][Constants.Metadata.OrderForm.SelectedCategories] as string).Split(',').Select(x =>
                    {
                        int i = 0;
                        Int32.TryParse(x, out i);
                        return i;
                    }).Where(x => x > 0).ToArray();
                    customer.SetCategories(s);
                }
                customer.SaveChanges();
            }
        }
    }
}