using System.Security.Principal;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface IPaymentCompleteHandler
    {
        void OnPaymentComplete(PurchaseOrder order, IIdentity identity);
        bool SendOrderReceipt(PurchaseOrder order);
        void ForwardOrderToErp(PurchaseOrder purchaseOrder);
        void AdjustStocks(PurchaseOrder order);
    }
}