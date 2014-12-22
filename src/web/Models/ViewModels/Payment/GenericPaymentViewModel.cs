/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using EPiServer.Web.Routing;

using Mediachase.Commerce;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.PaymentProviders;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;
using OxxCommerceStarterKit.Web.Business;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;

namespace OxxCommerceStarterKit.Web.Models.ViewModels.Payment
{
    public class GenericPaymentViewModel<T> : PageViewModel<T> where T: BasePaymentPage
    {
        protected static ILogger _log = LogManager.GetLogger();
        private Cart _currentCart = null;
        private Mediachase.Commerce.Orders.Payment _payment;
        private PaymentMethodDto _paymentMethod;

        
        /// <summary>
        /// Gets the order ID.
        /// </summary>
        /// <value>The order ID.</value>
        public string OrderID { get; protected set; }
        public string OrderInfo { get; set; }

       
        private Cart CurrentCart
        {
            get
            {
                if (_currentCart == null)
                {
                    _currentCart = new CartHelper(Cart.DefaultName).Cart;
                }

                return _currentCart;
            }
            set { _currentCart = value; }
        }

        /// <summary>
        /// Gets the amount.
        /// </summary>
        /// <value>The amount.</value>
        public string Amount
        {
            get
            {
                return (_payment != null) ? (_payment.Amount * 100).ToString("#") : string.Empty;
            }
        }

        /// <summary>
        /// Gets the currency code.
        /// </summary>
        /// <value>The currency code.</value>
        public Currency Currency
        {
            get
            {
                if (_payment == null)
                {
                    return string.Empty;
                }
                return string.IsNullOrEmpty(_payment.Parent.Parent.BillingCurrency) ?
                    SiteContext.Current.Currency : new Currency(_payment.Parent.Parent.BillingCurrency);
            }
        }


        /// <summary>
        /// Convert the site language to a language DIBS can support.
        /// </summary>
        /// <value>The current language in a format usable by DIBS.</value>
        public new string Language
        {
            get
            {
                if (SiteContext.Current.LanguageName.StartsWith("da", StringComparison.OrdinalIgnoreCase))
                    return "da";
                else if (SiteContext.Current.LanguageName.StartsWith("sv", StringComparison.OrdinalIgnoreCase))
                    return "sv";
                else if (SiteContext.Current.LanguageName.StartsWith("no", StringComparison.OrdinalIgnoreCase))
                    return "no";
                else if (SiteContext.Current.LanguageName.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    return "en";
                else if (SiteContext.Current.LanguageName.StartsWith("nl", StringComparison.OrdinalIgnoreCase))
                    return "nl";
                else if (SiteContext.Current.LanguageName.StartsWith("de", StringComparison.OrdinalIgnoreCase))
                    return "de";
                else if (SiteContext.Current.LanguageName.StartsWith("fr", StringComparison.OrdinalIgnoreCase))
                    return "fr";
                else if (SiteContext.Current.LanguageName.StartsWith("fi", StringComparison.OrdinalIgnoreCase))
                    return "fi";
                else if (SiteContext.Current.LanguageName.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                    return "es";
                else if (SiteContext.Current.LanguageName.StartsWith("it", StringComparison.OrdinalIgnoreCase))
                    return "it";
                else if (SiteContext.Current.LanguageName.StartsWith("fo", StringComparison.OrdinalIgnoreCase))
                    return "fo";
                else if (SiteContext.Current.LanguageName.StartsWith("pl", StringComparison.OrdinalIgnoreCase))
                    return "pl";
                return "en";
            }
        }


        public GenericPaymentViewModel(Guid paymentMethod, T currentPage, OrderInfo orderInfo, Cart cart) : base(currentPage)
        {
            PaymentMethodDto payment = PaymentManager.GetPaymentMethod(paymentMethod);
            _paymentMethod = payment;
            _currentCart = cart;

            DIBSPaymentGateway gw = new DIBSPaymentGateway();

            orderInfo.Merchant = gw.Merchant;
            
			Mediachase.Commerce.Orders.Payment[] payments;
			if (CurrentCart != null && CurrentCart.OrderForms != null && CurrentCart.OrderForms.Count > 0)
			{
				payments = CurrentCart.OrderForms[0].Payments.ToArray();
			}
			else
			{
				payments = new Mediachase.Commerce.Orders.Payment[0];
			}
            _payment = payments.FirstOrDefault(c => c.PaymentMethodId.Equals(payment.PaymentMethod.Rows[0]["PaymentMethodId"]));
            
            
            OrderID = orderInfo.OrderId;
            
        }

        private string GetViewUrl(ContentReference contentLink)
        {
            var url = UrlResolver.Current.GetUrl(
                contentLink,
                null,
                new VirtualPathArguments() {ContextMode = ContextMode.Default});
            return url;
        }
    }
}
