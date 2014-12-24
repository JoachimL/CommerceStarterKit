using System.Security.Principal;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public interface IPaymentCompleteHandler
    {
        void ProcessCompletedPayment(PurchaseOrderModel orderModel, IIdentity identity);
    }
}