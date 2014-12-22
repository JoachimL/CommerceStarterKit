﻿/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using AuthorizeNet;
using Mediachase.Commerce.Orders;
using Newtonsoft.Json;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Core.Repositories;
using LineItem = OxxCommerceStarterKit.Core.Objects.LineItem;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using EPiServer.Logging;

namespace OxxCommerceStarterKit.Core.Services
{
    public class OrderService : IOrderService
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private PurchaseOrder _purchaseOrder;

        public List<LineItem> GetItems()
        {
            return MapCartItems(_purchaseOrder.OrderForms[0].LineItems.ToArray(), "no");
        }

        private List<LineItem> MapCartItems(IEnumerable<Mediachase.Commerce.Orders.LineItem> lineItems, string language)
        {

            List<LineItem> items = new List<LineItem>();


            if (lineItems != null)
                foreach (Mediachase.Commerce.Orders.LineItem lineItem in lineItems)
                {
                    items.Add(new LineItem
                    {
                        Code = lineItem.CatalogEntryId,
                        Name = lineItem.GetStringValue(Constants.Metadata.LineItem.DisplayName),
                        ArticleNumber = lineItem.GetStringValue(Constants.Metadata.LineItem.ArticleNumber),
                        ImageUrl = lineItem.GetString(Constants.Metadata.LineItem.ImageUrl),
                        Color = lineItem.GetStringValue(Constants.Metadata.LineItem.Color),
                        ColorImageUrl = lineItem.GetStringValue(Constants.Metadata.LineItem.ColorImageUrl),
                        Description = lineItem.GetStringValue(Constants.Metadata.LineItem.Description),
                        Size = lineItem.GetStringValue(Constants.Metadata.LineItem.Size),
                        PlacedPrice = lineItem.PlacedPrice,
                        LineItemTotal = lineItem.Quantity * lineItem.PlacedPrice,
                        LineItemDiscount = lineItem.LineItemDiscountAmount,
                        LineItemOrderLevelDiscount = lineItem.OrderLevelDiscountAmount,
                        Quantity = Convert.ToInt32(lineItem.Quantity),
                        Url = lineItem.GetEntryLink(language)
                    });
                }

            return items;
        }

        public string GetCustomerEmail()
        {
            return _purchaseOrder.GetBillingEmail();
        }


        public Objects.SharedViewModels.PurchaseOrderModel GetOrderByTrackingNumber(string trackingNumber)
        {
            return MapToModel(new OrderRepository().GetOrderByTrackingNumber(trackingNumber));
        }

        public List<Objects.SharedViewModels.PurchaseOrderModel> GetOrdersByUserId(Guid customerId)
        {
            var orders = new OrderRepository().GetOrdersByUserId(customerId);
            if (orders == null)
                return Enumerable.Empty<PurchaseOrderModel>().ToList();
            return orders.Select(MapToModel).ToList();
        }

        private Objects.SharedViewModels.PurchaseOrderModel MapToModel(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
                return null;
            return new PurchaseOrderModel()
            {
                BackendOrderNumber = purchaseOrder.GetStringValue(Constants.Metadata.PurchaseOrder.BackendOrderNumber),
                Created = purchaseOrder.Created,
                OrderForms = purchaseOrder.OrderForms.OrEmpty().Select(MapOrderForm),
                OrderAddresses = purchaseOrder.OrderAddresses.OrEmpty().Select(MapOrderAddress),
                ShippingTotal = purchaseOrder.ShippingTotal,
                Status = purchaseOrder.Status,
                TaxTotal = purchaseOrder.TaxTotal,
                Total = purchaseOrder.Total,
                TrackingNumber = purchaseOrder.TrackingNumber,
                BillingEmail = GetBillingEmail(purchaseOrder),
                BillingPhone = GetBillingPhone(purchaseOrder),
                ProviderId = purchaseOrder.ProviderId,
                MarketId = purchaseOrder.MarketId
            };
        }

        private OrderFormModel MapOrderForm(OrderForm orderForm)
        {
            return new OrderFormModel()
            {
                Discounts = MapDiscounts(orderForm.Discounts),
                LineItems = orderForm.LineItems.Select(MapToModel),
                Payments = orderForm.Payments.OrEmpty().Select(MapToModel).ToArray(),
                Shipments = orderForm.Shipments.OrEmpty().Select(MapToModel).ToArray()
            };
        }

        private ShipmentModel MapToModel(Shipment shipment)
        {
            return new ShipmentModel()
            {
                Discounts = MapDiscounts(shipment.Discounts),
                ShipmentTrackingNumber = shipment.ShipmentTrackingNumber,
                ShippingDiscountAmount = shipment.ShippingDiscountAmount
            };
        }

        private DiscountModel[] MapDiscounts(ShipmentDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private PaymentModel MapToModel(Payment payment)
        {
            return new PaymentModel()
            {
                PaymentMethodName = payment.PaymentMethodName
            };
        }

        private static DiscountModel[] MapDiscounts(OrderFormDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private LineItemModel MapToModel(Mediachase.Commerce.Orders.LineItem item)
        {
            return new LineItemModel()
            {
                ArticleNumber = item.GetStringValue(Constants.Metadata.LineItem.ArticleNumber),
                CatalogEntryId = item.CatalogEntryId,
                Color = item.GetStringValue(Constants.Metadata.LineItem.Color),
                Description = item.GetStringValue(Constants.Metadata.LineItem.Description),
                Discounts = MapDiscounts(item.Discounts),
                DisplayName = item.DisplayName,
                ExtendedPrice = item.ExtendedPrice,
                LineItemDiscountAmount = item.LineItemDiscountAmount,
                OrderLevelDiscountAmount = item.OrderLevelDiscountAmount,
                Quantity = (int)item.Quantity,
                Size = item.GetStringValue(Constants.Metadata.LineItem.Size)
            };
        }

        private DiscountModel[] MapDiscounts(LineItemDiscountCollection discounts)
        {
            if (discounts == null || discounts.Count == 0)
                return new DiscountModel[0];
            var models = new List<DiscountModel>(discounts.Count);
            for (var ii = 0; ii < discounts.Count; ii++)
            {
                models.Add(new DiscountModel()
                {
                    DiscountCode = discounts[ii].DiscountCode
                });
            }
            return models.ToArray();
        }

        private OrderAddressModel MapOrderAddress(OrderAddress address)
        {
            return new OrderAddressModel()
            {
                City = address.City,
                CountryCode = address.CountryCode,
                DeliveryServicePoint = GetDeliveryServicePointFrom(address),
                FirstName = address.FirstName,
                LastName = address.LastName,
                Id = address.Id,
                Line1 = address.Line1,
                Name = address.Name,
                PostalCode = address.PostalCode
            };
        }

        private string GetDeliveryServicePointFrom(OrderAddress shippingAddress)
        {
            if (string.IsNullOrWhiteSpace((string)shippingAddress[Constants.Metadata.Address.DeliveryServicePoint]))
                return string.Empty;
            try
            {
                var deliveryServicePoint =
                    JsonConvert.DeserializeObject<ServicePoint>(
                        (string)shippingAddress[Constants.Metadata.Address.DeliveryServicePoint]);
                return deliveryServicePoint.Name;
            }
            catch (Exception ex)
            {
                // Todo: Move to method with more documentation about why this can fail
                Log.Error("Error during deserializing delivery location", ex);
            }
            return string.Empty;
        }

        private string GetBillingEmail(PurchaseOrder purchaseOrder)
        {
            try
            {
                return purchaseOrder.GetBillingEmail();
            }
            catch (Exception ex)
            {
                // TODO: Inspect this, do we need a try catch here?
                Log.Error("Error getting email for customer", ex);
            }
            return string.Empty;
        }

        private string GetBillingPhone(PurchaseOrder purchaseOrder)
        {
            try
            {
                return purchaseOrder.GetBillingPhone();
            }
            catch (Exception ex)
            {
                // TODO: Inspect this, do we need a try catch here?
                Log.Error("Error getting phone for customer", ex);
            }
            return string.Empty;
        }

    }
}
