using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using OxxCommerceStarterKit.Core.Objects;
using OxxCommerceStarterKit.Web.Business.Delivery;
using OxxCommerceStarterKit.Web.Services;

namespace OxxCommerceStarterKit.Web.Api
{
    public class DeliveryLocationsController : BaseApiController
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly IPostNordClient _postNordClient;

        public DeliveryLocationsController()
        {
            _postNordClient = ServiceLocator.Current.GetInstance<IPostNordClient>();
        }

        public DeliveryLocationsController(IPostNordClient postNordClient)
        {
            _postNordClient = postNordClient;
        }

        public async Task<IEnumerable<DeliveryLocation>> Get(string streetAddress, string city, string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                throw new HttpException(400, "Postal code missing.");

            return await GetLocationsFromServiceAsync(city, postalCode, ParseStreetAddress(streetAddress));
        }

        private async Task<IEnumerable<DeliveryLocation>> GetLocationsFromServiceAsync(string city, string postalCode, StreetAddress parsedAddress)
        {
            try
            {
                return (await
                    _postNordClient.FindNearestByAddress(new RegionInfo("NO"), city, postalCode, parsedAddress.Street,
                        parsedAddress.Number)).
                    Select(CreateDeliveryLocationFrom);
            }
            catch (Exception ex)
            {
                Log.Error("Error occured when getting service points from Postnord.", ex);
                return Enumerable.Empty<DeliveryLocation>();
            }
        }

        private static StreetAddress ParseStreetAddress(string streetAddress)
        {
            string streetNumber = null;
            if (!string.IsNullOrEmpty(streetAddress))
            {
                streetAddress = streetAddress.TrimEnd();


                // Parse the street name and number (if any) from the street address

                var i = streetAddress.Length - 1;
                bool hasDigit = false;

                for (; 0 <= i; --i)
                {
                    if (char.IsDigit(streetAddress[i]))
                    {
                        hasDigit = true;
                        continue;
                    }

                    if (!char.IsLetter(streetAddress[i]))
                    {
                        break;
                    }
                }

                ++i;


                if (hasDigit && i < streetAddress.Length)
                {
                    streetNumber = streetAddress.Substring(i);
                    streetAddress = streetAddress.Substring(0, i).Trim();
                }
                else
                {
                    streetNumber = null;
                }
            }
            return new StreetAddress(streetAddress, streetNumber);
        }

        private static DeliveryLocation CreateDeliveryLocationFrom(PostNord.ServicePoint sp)
        {
            return new DeliveryLocation(
                new ServicePoint() { Id = sp.Id, Name = sp.Name, Address = string.Concat(sp.DeliveryAddress.StreetName, ' ', sp.DeliveryAddress.StreetNumber), City = sp.DeliveryAddress.City, PostalCode = sp.DeliveryAddress.PostalCode },
                sp.Name);
        }
    }
}