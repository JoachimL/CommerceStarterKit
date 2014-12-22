using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace OxxCommerceStarterKit.Web
{
    public interface IIdentityProvider
    {
        IIdentity GetIdentity();
    }
}