using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public interface IStockUpdater
    {
        void AdjustStocks(PurchaseOrderModel order);
    }
}