using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface IGoogleAnalyticsTracker
    {
        void TrackAfterPayment(ReceiptViewModel model);
    }
}