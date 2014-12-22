using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxxCommerceStarterKit.Core.Objects.SharedViewModels
{
    public class OrderFormModel
    {
        public IEnumerable<LineItemModel> LineItems { get; set; }
        public PaymentModel[] Payments { get; set; }
        public ShipmentModel[] Shipments { get; set; }
        public DiscountModel[] Discounts { get; set; }
    }
}
