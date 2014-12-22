﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website.Helpers;
using OxxCommerceStarterKit.Core.Extensions;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;
using OxxCommerceStarterKit.Core.PaymentProviders.Payment;
using OxxCommerceStarterKit.Core.Repositories.Interfaces;
using EPiServer.Logging;
using System.Security.Principal;
using OxxCommerceStarterKit.Core.Services;

namespace OxxCommerceStarterKit.Web.Controllers
{
    public class DibsPaymentProcessor : IDibsPaymentProcessor
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly IOrderService _orderService;
        private readonly IPaymentCompleteHandler _paymentCompleteHandler;

        public DibsPaymentProcessor(IOrderService orderService, IPaymentCompleteHandler paymentCompleteHandler)
        {
            _orderService = orderService;
            _paymentCompleteHandler = paymentCompleteHandler;
        }

        public DibsPaymentProcessingResult ProcessPaymentResult(DibsPaymentResult result, IIdentity identity)
        {
            var cartHelper = new CartHelper(Cart.DefaultName);
            if (cartHelper.Cart.OrderForms.Count == 0) 
                return null;

            var payment = cartHelper.Cart.OrderForms[0].Payments[0] as DibsPayment;
            if (payment != null)
            {
                payment.CardNumberMasked = result.CardNumberMasked;
                payment.CartTypeName = result.CardTypeName;
                payment.TransactionID = result.Transaction;
                payment.TransactionType = TransactionType.Authorization.ToString();
                payment.Status = result.Status;
                cartHelper.Cart.Status = DIBSPaymentGateway.PaymentCompleted;
            }
            else
            {
                throw new Exception("Not a DIBS Payment");
            }

            var orderNumber = cartHelper.Cart.GeneratePredictableOrderNumber();

            Log.Debug("Order placed - order number: " + orderNumber);

            cartHelper.Cart.OrderNumberMethod = CartExtensions.GeneratePredictableOrderNumber;

            var results = OrderGroupWorkflowManager.RunWorkflow(cartHelper.Cart, OrderGroupWorkflowManager.CartCheckOutWorkflowName);
            var message = string.Join(", ", OrderGroupWorkflowManager.GetWarningsFromWorkflowResult(results));

            if (message.Length == 0)
            {
                cartHelper.Cart.SaveAsPurchaseOrder();
                cartHelper.Cart.Delete();
                cartHelper.Cart.AcceptChanges();
            }

            var order = _orderService.GetOrderByTrackingNumber(orderNumber);

            // Must be run after order is complete, 
            // This will release the order for shipment and 
            // send the order receipt by email
            _paymentCompleteHandler.OnPaymentComplete(order, identity);

            return new DibsPaymentProcessingResult(order, message);
        }
    }
}
