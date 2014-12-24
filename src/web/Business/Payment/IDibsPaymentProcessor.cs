using System.Security.Principal;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public interface IDibsPaymentProcessor
    {
        DibsPaymentProcessingResult ProcessPaymentResult(DibsPaymentResult result, IIdentity identity);
    }
}