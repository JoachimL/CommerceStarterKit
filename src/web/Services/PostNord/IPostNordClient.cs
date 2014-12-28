using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OxxCommerceStarterKit.Web.Services
{
    public interface IPostNordClient
    {
        Task<IEnumerable<PostNord.ServicePoint>> FindNearestByAddress(RegionInfo regionInfo, string city, string postalCode,
            string streetName = null, string streetNumber = null, int numberOfServicePoints = 5,
            string srId = "EPSG:4326");
    }
}
