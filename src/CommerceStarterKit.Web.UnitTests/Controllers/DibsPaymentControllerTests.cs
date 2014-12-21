using System.Security.Principal;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using Mediachase.Commerce.Orders;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxxCommerceStarterKit.Core;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;
using OxxCommerceStarterKit.Web;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Controllers;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using Ploeh.AutoFixture;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using Should;

namespace CommerceStarterKit.Web.Controllers
{
    public class DibsPaymentControllerTests
    {
        private static readonly Fixture Fixture = new Fixture();
        private DibsPaymentController _sut;
        private Mock<IPaymentCompleteHandler> _paymentCompleteHandlerMock;
        private Mock<IContentRepository> _contentRepositoryMock;
        private Mock<IDibsPaymentProcessor> _dibsPaymentProcessorMock;
        private Mock<IIdentityProvider> _identityProvider;
        private SettingsBlock _settingsBlock;

        static DibsPaymentControllerTests()
        {
            var receiptPage = new PageReference(Fixture.Create<int>());
            Fixture.Register(() => new SettingsBlock()
            {
                ReceiptPage = receiptPage
            });

            RegisterPurchaseOrderCreationRules();
            Fixture.Register(
                () => new DibsPaymentProcessingResult(Fixture.Create<PurchaseOrder>(), Fixture.Create<string>()));
        }

        private static void RegisterPurchaseOrderCreationRules()
        {
            Fixture.Register<PurchaseOrder>(
                () => Fixture.Build<PurchaseOrderStub>()
                    .OmitAutoProperties()
                    .With(x => x.TrackingNumber)
                    .With(x => x.Created)
                    .With(x => x.Status)
                    .With(x => x.Total)
                    .With(x => x.ShippingTotal)
                    .With(x => x.TaxTotal)

                    .Create());
        }

        [SetUp]
        public virtual void SetUp()
        {
            _paymentCompleteHandlerMock = new Mock<IPaymentCompleteHandler>();
            _dibsPaymentProcessorMock = new Mock<IDibsPaymentProcessor>();
            _settingsBlock = Fixture.Create<SettingsBlock>();
            SetUpSiteConfigurationMock();
            SetUpContentRepository();
            _identityProvider = new Mock<IIdentityProvider>();
            _sut = new DibsPaymentController(_identityProvider.Object, _paymentCompleteHandlerMock.Object, _contentRepositoryMock.Object, _dibsPaymentProcessorMock.Object, _siteConfigurationMock.Object);
        }

        //private void SetUpSiteConfigurationMock()
        //{
        //    _siteConfigurationMock = new Mock<ISiteSettingsProvider>();
        //    _siteConfigurationMock.Setup(c => c.GetSettings()).Returns(_settingsBlock);
        //}

        private void SetUpContentRepository()
        {
            _contentRepositoryMock = new Mock<IContentRepository>();
            _contentRepositoryMock.Setup(c => c.Get<ReceiptPage>(_settingsBlock.ReceiptPage))
                .Returns(new Mock<ReceiptPage>().Object);
        }

        public class When_processing_a_completed_payment : DibsPaymentControllerTests
        {
            [Test]
            public void _then_the_payment_result_is_passed_to_the_payment_complete_processor()
            {
                var paymentResponse = Fixture.Create<DibsPaymentResult>();
                var processingResult = Fixture.Create<DibsPaymentProcessingResult>();
                _dibsPaymentProcessorMock.Setup(x => x.ProcessPaymentResult(paymentResponse, It.IsAny<IIdentity>()))
                    .Returns(processingResult);
                _

                ViewResult result = (ViewResult)_sut.ProcessPayment(new DibsPaymentPage(), paymentResponse);
                var resultModel = (ReceiptViewModel)result.Model;

                resultModel.CheckoutMessage.ShouldEqual(processingResult.Message);
            }
        }

    }
}
