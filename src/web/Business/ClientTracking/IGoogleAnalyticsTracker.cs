using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Business.ClientTracking
{
    public interface IGoogleAnalyticsTracker
    {
        void TrackAfterPayment(ReceiptViewModel model);
    }
}