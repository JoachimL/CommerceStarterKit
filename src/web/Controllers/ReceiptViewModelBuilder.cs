using System.Web.Mvc;
using EPiServer;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Business.Analytics;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class ReceiptViewModelBuilder : IReceiptViewModelBuilder
    {
        private readonly IContentRepository _contentRepository;
        private readonly ISiteSettingsProvider _siteConfiguration;

        public ReceiptViewModelBuilder(IContentRepository contentRepository, ISiteSettingsProvider siteConfiguration)
        {
            _contentRepository = contentRepository;
            _siteConfiguration = siteConfiguration;
        }

        public ReceiptViewModel BuildFor(DibsPaymentProcessingResult processingResult)
        {
            ReceiptPage receiptPage = _contentRepository.Get<ReceiptPage>(_siteConfiguration.GetSettings().ReceiptPage);
            ReceiptViewModel model = new ReceiptViewModel(receiptPage);
            model.CheckoutMessage = processingResult.Message;
            model.Order = new OrderViewModel(processingResult.Order);

            // Track successfull order in Google Analytics
            TrackAfterPayment(model);
            return model;
        }

        private void TrackAfterPayment(ReceiptViewModel model)
        {
            // Track Analytics 
            GoogleAnalyticsTracking tracking = new GoogleAnalyticsTracking(ControllerContext.HttpContext);

            // Add the products
            int i = 1;
            foreach (OrderLineViewModel orderLine in model.Order.OrderLines)
            {
                if (string.IsNullOrEmpty(orderLine.Code) == false)
                {
                    tracking.ProductAdd(code: orderLine.Code,
                        name: orderLine.Name,
                        quantity: orderLine.Quantity,
                        price: (double)orderLine.Price,
                        position: i
                        );
                    i++;
                }
            }

            // And the transaction itself
            tracking.Purchase(model.Order.OrderNumber,
                null, (double)model.Order.TotalAmount, (double)model.Order.Tax, (double)model.Order.Shipping);
        }
    }
}