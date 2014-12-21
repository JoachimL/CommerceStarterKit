using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class DibsPaymentProcessingResult
    {
        private readonly string _message;
        private readonly PurchaseOrder _order;

        public DibsPaymentProcessingResult(PurchaseOrder order, string message)
        {
            _message = message;
            _order = order;
        }

        public string Message { get { return _message; } }
        public PurchaseOrder Order { get { return _order; } }
    }
}