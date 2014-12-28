using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxxCommerceStarterKit.Web.Business.Delivery;
using OxxCommerceStarterKit.Web.Services;
using Should;

namespace OxxCommerceStarterKit.Web.Api
{
    static class DeliveryLocationExtensions
    {
        public static void ShouldBeEquivalentOf(this DeliveryLocation location, PostNord.ServicePoint servicePoint)
        {
            location.ShouldNotBeNull();
            location.text.ShouldEqual(servicePoint.Name);
            location.value.Address.ShouldEqual(string.Concat(servicePoint.DeliveryAddress.StreetName, ' ', servicePoint.DeliveryAddress.StreetNumber));
        }
    }
}
