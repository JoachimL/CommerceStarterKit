using System.Security.Principal;
using Mediachase.Commerce.Orders;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface IPaymentCompleteHandler
    {
        void OnPaymentComplete(PurchaseOrderModel orderModel, IIdentity identity);
        bool SendOrderReceipt(PurchaseOrderModel order);
        void ForwardOrderToErp(PurchaseOrder purchaseOrder);
        void AdjustStocks(PurchaseOrder order);
    }
}