using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OxxCommerceStarterKit.Web.Api
{
    public class StreetAddress
    {
        public string Street { get; private set; }
        public string Number { get; private set; }

        public StreetAddress(string street, string number)
        {
            Street = street;
            Number = number;
        }
    }
}
