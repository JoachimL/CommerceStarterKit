using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using EPiServer.Shell.Web;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public interface IHttpContextProvider
    {
        HttpContextBase GetContext();
    }

    public class HttpContextProvider : IHttpContextProvider
    {

        public HttpContextBase GetContext()
        {
            return HttpContext.Current.GetHttpContextBase();
        }
    }
}
