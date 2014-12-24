using System;
using System.Security.Principal;
using EPiServer.Logging;
using OxxCommerceStarterKit.Core.Email;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;
using OxxCommerceStarterKit.Core.Services;

namespace OxxCommerceStarterKit.Web.Business.Payment
{
    public class PaymentCompleteHandler : IPaymentCompleteHandler
    {
        private static readonly ILogger Log = LogManager.GetLogger();
        private readonly IEmailService _emailService;
        private readonly IOrderService _orderService;
        private readonly IStockUpdater _stockUpdater;

        public PaymentCompleteHandler(IEmailService emailService, IOrderService orderService, IStockUpdater stockUpdater)
        {
            _emailService = emailService;
            _orderService = orderService;
            _stockUpdater = stockUpdater;
        }

        public void ProcessCompletedPayment(PurchaseOrderModel orderModel, IIdentity identity)
        {
            _orderService.FinalizeOrder(orderModel.TrackingNumber, identity);

            SendAndLogOrderReceipt(orderModel);

            AttemptStockAdjustment(orderModel);

            ForwardOrderToErp(orderModel);
        }

        private void SendAndLogOrderReceipt(PurchaseOrderModel orderModel)
        {
            var sendOrderReceiptResult = SendOrderReceipt(orderModel);
            Log.Debug("Sending receipt e-mail - " + (sendOrderReceiptResult ? "success" : "failed"));
        }

        private bool SendOrderReceipt(PurchaseOrderModel order)
        {
            return _emailService.SendOrderReceipt(order);
        }

        private void AttemptStockAdjustment(PurchaseOrderModel orderModel)
        {
            try
            {
                // Not extremely important that this succeeds. 
                // Stocks are continually adjusted from ERP.
                _stockUpdater.AdjustStocks(orderModel);
            }
            catch (Exception e)
            {
                Log.Error("Error adjusting inventory after purchase.", e);
            }
        }

        public void ForwardOrderToErp(PurchaseOrderModel purchaseOrder)
        {
            // TODO: Implement for your solution
        }
    }
}