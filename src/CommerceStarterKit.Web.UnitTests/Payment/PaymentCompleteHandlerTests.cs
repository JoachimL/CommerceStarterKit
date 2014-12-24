using System.Security.Principal;
using AuthorizeNet;
using Moq;
using NUnit.Framework;
using OxxCommerceStarterKit.Core.Customers;
using OxxCommerceStarterKit.Core.Email;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using OxxCommerceStarterKit.Core.Services;
using OxxCommerceStarterKit.Web.Business.Payment;
using OxxCommerceStarterKit.Web.Services.Email;
using Ploeh.AutoFixture;

namespace OxxCommerceStarterKit.Web.Payment
{
    public class PaymentCompleteHandlerTests
    {
        private static Fixture Fixture = new Fixture();
        private Mock<IEmailService> _emailServiceMock;
        private Mock<IOrderService> _orderServiceMock;
        private Mock<IStockUpdater> _stockUpdaterMock;
        private PaymentCompleteHandler _sut;

        [SetUp]
        public virtual void SetUp()
        {
            _emailServiceMock = new Mock<IEmailService>();
            _orderServiceMock = new Mock<IOrderService>();
            _stockUpdaterMock = new Mock<IStockUpdater>();
            _sut = new PaymentCompleteHandler(_emailServiceMock.Object, _orderServiceMock.Object, _stockUpdaterMock.Object);
        }

        public class When_the_payment_is_complete : PaymentCompleteHandlerTests
        {
            private PurchaseOrderModel _orderModel;
            private IIdentity _currentUser;
            public override void SetUp()
            {
                base.SetUp();

                _orderModel = Fixture.Create<PurchaseOrderModel>();
                _currentUser = new Mock<IIdentity>().Object;
            }

            [Test]
            public void _then_the_order_is_finalized_by_its_tracking_number()
            {
                _sut.ProcessCompletedPayment(_orderModel, _currentUser);

                _orderServiceMock.Verify(s => s.FinalizeOrder(_orderModel.TrackingNumber, It.IsAny<IIdentity>()));
            }

            [Test]
            public void _then_the_current_user_is_passed_to_the_order_service()
            {
                _sut.ProcessCompletedPayment(_orderModel, _currentUser);

                _orderServiceMock.Verify(s => s.FinalizeOrder(It.IsAny<string>(), _currentUser));
            }

            [Test]
            public void _then_a_receipt_for_the_order_is_sent()
            {
                _sut.ProcessCompletedPayment(_orderModel, _currentUser);

                _emailServiceMock.Verify(r => r.SendOrderReceipt(_orderModel));
            }

            [Test]
            public void _then_the_stock_is_updated_based_on_the_order()
            {
                _sut.ProcessCompletedPayment(_orderModel, _currentUser);

                _stockUpdaterMock.Verify(u => u.AdjustStocks(_orderModel));
            }
        }
    }
}
