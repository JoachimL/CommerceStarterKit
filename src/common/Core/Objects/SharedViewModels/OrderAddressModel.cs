using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxxCommerceStarterKit.Core.Objects.SharedViewModels
{
    public class OrderAddressModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Line1 { get; set; }

        public string PostalCode { get; set; }

        public string City { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CountryCode { get; set; }

        public string DeliveryServicePoint { get; set; }
    }
}
