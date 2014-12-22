using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface ICustomerFactory
    {
        CustomerContact CreateCustomer(string email, string password, string phone, OrderAddress billingAddress,
            OrderAddress shippingAddress, bool hasPassword, Action<MembershipCreateStatus> userCreationFailed);
    }
}
