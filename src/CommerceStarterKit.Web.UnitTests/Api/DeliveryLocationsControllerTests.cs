using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Moq;
using NUnit.Framework;
using OxxCommerceStarterKit.Web.Services;
using Ploeh.AutoFixture;
using Should;

namespace OxxCommerceStarterKit.Web.Api
{
    public class DeliveryLocationsControllerTests
    {
        private static readonly IFixture Fixture = new Fixture();
        private DeliveryLocationsController _sut;
        private Mock<IPostNordClient> _postNordClientMock;

        static DeliveryLocationsControllerTests()
        {
            Fixture.Register<RegionInfo>(() => new RegionInfo("NO"));
            new SupportMutableValueTypesCustomization().Customize(Fixture);
            //Fixture.Register<Address>(() => Fixture.Build<PostNord.Address>().With(a => a.Region, new RegionInfo("NO"));
        }

        [SetUp]
        public virtual void SetUp()
        {
            _postNordClientMock = new Mock<IPostNordClient>();
            _sut = new DeliveryLocationsController(_postNordClientMock.Object);
        }

        public class When_getting_delivery_locations : DeliveryLocationsControllerTests
        {
            private string _street;
            private string _number;
            private string _streetAddress;
            public override void SetUp()
            {
                base.SetUp();

                _street = "street name";
                _number = Fixture.Create<int>().ToString();
                _streetAddress = string.Concat(_street, " ", _number);
            }
            public class _without_providing_a_postal_code : When_getting_delivery_locations
            {
                [Test]
                [ExpectedException(typeof(HttpException))]
                [TestCase("")]
                [TestCase(null)]
                [TestCase("   ")]
                public async Task _then_an_exception_is_thrown(string empty)
                {
                    await _sut.Get(Fixture.Create<string>(), Fixture.Create<string>(), empty);
                }
            }

            public class _with_a_street_address_with_a_house_number : When_getting_delivery_locations
            {
                [Test]
                public async Task _then_the_number_is_recognized_and_provided_to_the_locations_service()
                {
                    var serviceResult = Fixture.CreateMany<PostNord.ServicePoint>().ToList();
                    _postNordClientMock.Setup(
                        c =>
                            c.FindNearestByAddress(It.IsAny<RegionInfo>(), It.IsAny<string>(), It.IsAny<string>(),
                                _street, _number, It.IsAny<int>(), It.IsAny<string>()))
                                .ReturnsAsync(serviceResult);

                    var result = await _sut.Get(_streetAddress, Fixture.Create<string>(), Fixture.Create<string>());

                    result.Count().ShouldEqual(serviceResult.Count());
                }
            }

            public class _with_a_street_address_with_no_house_number : When_getting_delivery_locations
            {
                [Test]
                public async Task _then_no_number_is_passed__to_the_locations_service()
                {
                    var serviceResult = Fixture.CreateMany<PostNord.ServicePoint>().ToList();
                    _postNordClientMock.Setup(
                        c =>
                            c.FindNearestByAddress(It.IsAny<RegionInfo>(), It.IsAny<string>(), It.IsAny<string>(),
                                _street, null, It.IsAny<int>(), It.IsAny<string>()))
                                .ReturnsAsync(serviceResult);

                    var result = await _sut.Get(_street, Fixture.Create<string>(), Fixture.Create<string>());

                    result.Count().ShouldEqual(serviceResult.Count());
                }
            }

            public class _and_post_nord_returns_locations : When_getting_delivery_locations
            {
                [Test]
                public async Task _then_service_points_are_returned_to_the_clitn()
                {
                    var serviceResult = Fixture.CreateMany<PostNord.ServicePoint>().ToList();
                    _postNordClientMock.Setup(
                        c =>
                            c.FindNearestByAddress(It.IsAny<RegionInfo>(), It.IsAny<string>(), It.IsAny<string>(),
                                It.IsAny<string>(), null, It.IsAny<int>(), It.IsAny<string>()))
                                .ReturnsAsync(serviceResult);

                    var result = await _sut.Get(_street, Fixture.Create<string>(), Fixture.Create<string>());

                    foreach (var servicePoint in serviceResult)
                        result.Single(r => r.value.Id == servicePoint.Id).ShouldBeEquivalentOf(servicePoint);
                }
            }

            public class _and_post_nord_throws_an_exception : When_getting_delivery_locations
            {
                [Test]
                public async Task _then_an_empty_list_is_returned()
                {
                    _postNordClientMock.Setup(
                        c =>
                            c.FindNearestByAddress(It.IsAny<RegionInfo>(), It.IsAny<string>(), It.IsAny<string>(),
                                It.IsAny<string>(), null, It.IsAny<int>(), It.IsAny<string>()))
                        .ThrowsAsync(new Exception());
                    var result = await _sut.Get(_street, Fixture.Create<string>(), Fixture.Create<string>());
                    result.ShouldBeEmpty();
                }
            }
        }
    }
}
