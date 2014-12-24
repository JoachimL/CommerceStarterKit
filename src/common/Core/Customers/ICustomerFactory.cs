using System;
using System.Web.Security;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Core.Customers
{
    public interface ICustomerFactory
    {
        CustomerContact CreateCustomer(string email, string password, string phone, OrderAddress billingAddress,
            OrderAddress shippingAddress, bool hasPassword, Action<MembershipCreateStatus> userCreationFailed);
    }
}
