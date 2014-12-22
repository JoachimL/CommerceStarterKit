using System.Web.Mvc;
using EPiServer;
using Mediachase.Commerce;
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
        private readonly ICurrentMarket _currentMarket;

        public ReceiptViewModelBuilder(IContentRepository contentRepository, ISiteSettingsProvider siteConfiguration, ICurrentMarket currentMarket)
        {
            _contentRepository = contentRepository;
            _siteConfiguration = siteConfiguration;
            _currentMarket = currentMarket;
        }

        public ReceiptViewModel BuildFor(DibsPaymentProcessingResult processingResult)
        {
            var receiptPage = _contentRepository.Get<ReceiptPage>(_siteConfiguration.GetSettings().ReceiptPage);
            var model = new ReceiptViewModel(receiptPage);
            model.CheckoutMessage = processingResult.Message;
            model.Order = new OrderViewModel(_currentMarket.GetCurrentMarket().DefaultCurrency.Format, processingResult.Order);
            return model;
        }
    }
}