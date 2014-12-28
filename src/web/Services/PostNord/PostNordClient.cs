using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OxxCommerceStarterKit.Web.Services
{
    public class PostNordClient : IPostNordClient
    {
        public Task<IEnumerable<PostNord.ServicePoint>> FindNearestByAddress(System.Globalization.RegionInfo regionInfo, string city, string postalCode, string streetName = null, string streetNumber = null, int numberOfServicePoints = 5, string srId = "EPSG:4326")
        {
            return PostNord.FindNearestByAddress(regionInfo, city, postalCode, streetName, streetNumber,
                numberOfServicePoints, srId);
        }
    }
}