using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OxxCommerceStarterKit.Web.Models.ViewModels;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface IReceiptViewModelBuilder
    {
        ReceiptViewModel BuildFor(DibsPaymentProcessingResult processingResult);
    }
}