using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public class DibsPaymentProcessingResult
    {
        private readonly string _message;
        private readonly PurchaseOrderModel _order;

        public DibsPaymentProcessingResult(PurchaseOrderModel order, string message)
        {
            _message = message;
            _order = order;
        }

        public string Message { get { return _message; } }
        public PurchaseOrderModel Order { get { return _order; } }
    }
}