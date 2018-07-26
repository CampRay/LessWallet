using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayPalStandard.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Text.RegularExpressions;
using Nop.Services.Security;
using Nop.Services.Customers;
using System.Web;
using Nop.Web.Framework.Kendoui;
using Braintree;
using Nop.Services.Catalog;
using System.Collections;

namespace Nop.Plugin.Payments.PayPalStandard.Controllers
{
    public class PaymentPayPalStandardController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly PayPalStandardPaymentSettings _payPalStandardPaymentSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IEncryptionService _encryptionService;
        private readonly ICustomerService _customerService;
        private readonly HttpContextBase _httpContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;

        public PaymentPayPalStandardController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger, 
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            PayPalStandardPaymentSettings payPalStandardPaymentSettings,
            ShoppingCartSettings shoppingCartSettings,
            IEncryptionService encryptionService,
            ICustomerService customerService,
            HttpContextBase httpContext, IShoppingCartService shoppingCartService, IProductService productService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._payPalStandardPaymentSettings = payPalStandardPaymentSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._encryptionService = encryptionService;
            this._customerService = customerService;
            this._httpContext = httpContext;
            this._shoppingCartService = shoppingCartService;
            this._productService = productService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.UseSandbox = payPalStandardPaymentSettings.UseSandbox;
            model.BusinessEmail = payPalStandardPaymentSettings.BusinessEmail;
            model.PdtToken = payPalStandardPaymentSettings.PdtToken;
            model.PdtValidateOrderTotal = payPalStandardPaymentSettings.PdtValidateOrderTotal;
            model.AdditionalFee = payPalStandardPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = payPalStandardPaymentSettings.AdditionalFeePercentage;
            model.PassProductNamesAndTotals = payPalStandardPaymentSettings.PassProductNamesAndTotals;
            model.EnableIpn = payPalStandardPaymentSettings.EnableIpn;
            model.IpnUrl = payPalStandardPaymentSettings.IpnUrl;
            model.AddressOverride = payPalStandardPaymentSettings.AddressOverride;
            model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage = payPalStandardPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage;

            model.MerchantId = payPalStandardPaymentSettings.MerchantId;
            model.PublicKey = payPalStandardPaymentSettings.PublicKey;
            model.PrivateKey = payPalStandardPaymentSettings.PrivateKey;
            model.MerchantAccountId = payPalStandardPaymentSettings.MerchantAccountId;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope);
                model.BusinessEmail_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope);
                model.PdtToken_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PdtToken, storeScope);
                model.PdtValidateOrderTotal_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
                model.EnableIpn_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.EnableIpn, storeScope);
                model.IpnUrl_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.IpnUrl, storeScope);
                model.AddressOverride_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.AddressOverride, storeScope);
                model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage, storeScope);

                model.MerchantId_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.MerchantId, storeScope);
                model.PublicKey_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PublicKey, storeScope);
                model.PrivateKey_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.PrivateKey, storeScope);
                model.MerchantAccountId_OverrideForStore = _settingService.SettingExists(payPalStandardPaymentSettings, x => x.MerchantAccountId, storeScope);

            }

            return View("~/Plugins/Payments.PayPalStandard/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            //save settings
            payPalStandardPaymentSettings.UseSandbox = model.UseSandbox;
            payPalStandardPaymentSettings.BusinessEmail = model.BusinessEmail;
            payPalStandardPaymentSettings.PdtToken = model.PdtToken;
            payPalStandardPaymentSettings.PdtValidateOrderTotal = model.PdtValidateOrderTotal;
            payPalStandardPaymentSettings.AdditionalFee = model.AdditionalFee;
            payPalStandardPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            payPalStandardPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            payPalStandardPaymentSettings.EnableIpn = model.EnableIpn;
            payPalStandardPaymentSettings.IpnUrl = model.IpnUrl;
            payPalStandardPaymentSettings.AddressOverride = model.AddressOverride;
            payPalStandardPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage = model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage;

            payPalStandardPaymentSettings.MerchantId = model.MerchantId;
            payPalStandardPaymentSettings.PublicKey = model.PublicKey;
            payPalStandardPaymentSettings.PrivateKey = model.PrivateKey;
            payPalStandardPaymentSettings.MerchantAccountId = model.MerchantAccountId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.BusinessEmail, model.BusinessEmail_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.PdtToken, model.PdtToken_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.PdtValidateOrderTotal, model.PdtValidateOrderTotal_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.EnableIpn, model.EnableIpn_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.IpnUrl, model.IpnUrl_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.AddressOverride, model.AddressOverride_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage, model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.PublicKey, model.PublicKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.PrivateKey, model.PrivateKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(payPalStandardPaymentSettings, x => x.MerchantAccountId, model.MerchantAccountId_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
        [ValidateInput(false)]
        public ActionResult RoundingWarning(bool passProductNamesAndTotals)
        {
            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = _localizationService.GetResource("Plugins.Payments.PayPalStandard.RoundingWarning") }, JsonRequestBehavior.AllowGet);

            return Json(new { Result = string.Empty }, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.PayPalStandard/Views/PaymentInfo.cshtml");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            var tx = _webHelper.QueryString<string>("tx");
            Dictionary<string, string> values;
            string response;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard") as PayPalStandardPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            if (processor.GetPdtDetails(tx, out values, out response))
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    decimal mc_gross = decimal.Zero;
                    try
                    {
                        mc_gross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                    }
                    catch (Exception exc)
                    {
                        _logger.Error("PayPal PDT. Error getting mc_gross", exc);
                    }

                    string payer_status = string.Empty;
                    values.TryGetValue("payer_status", out payer_status);
                    string payment_status = string.Empty;
                    values.TryGetValue("payment_status", out payment_status);
                    string pending_reason = string.Empty;
                    values.TryGetValue("pending_reason", out pending_reason);
                    string mc_currency = string.Empty;
                    values.TryGetValue("mc_currency", out mc_currency);
                    string txn_id = string.Empty;
                    values.TryGetValue("txn_id", out txn_id);
                    string payment_type = string.Empty;
                    values.TryGetValue("payment_type", out payment_type);
                    string payer_id = string.Empty;
                    values.TryGetValue("payer_id", out payer_id);
                    string receiver_id = string.Empty;
                    values.TryGetValue("receiver_id", out receiver_id);
                    string invoice = string.Empty;
                    values.TryGetValue("invoice", out invoice);
                    string payment_fee = string.Empty;
                    values.TryGetValue("payment_fee", out payment_fee);

                    var sb = new StringBuilder();
                    sb.AppendLine("Paypal PDT:");
                    sb.AppendLine("mc_gross: " + mc_gross);
                    sb.AppendLine("Payer status: " + payer_status);
                    sb.AppendLine("Payment status: " + payment_status);
                    sb.AppendLine("Pending reason: " + pending_reason);
                    sb.AppendLine("mc_currency: " + mc_currency);
                    sb.AppendLine("txn_id: " + txn_id);
                    sb.AppendLine("payment_type: " + payment_type);
                    sb.AppendLine("payer_id: " + payer_id);
                    sb.AppendLine("receiver_id: " + receiver_id);
                    sb.AppendLine("invoice: " + invoice);
                    sb.AppendLine("payment_fee: " + payment_fee);

                    var newPaymentStatus = PaypalHelper.GetPaymentStatus(payment_status, pending_reason);
                    sb.AppendLine("New payment status: " + newPaymentStatus);

                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //load settings for a chosen store scope
                    var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                    var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

                    //validate order total
                    var orderTotalSentToPayPal = order.GetAttribute<decimal?>("OrderTotalSentToPayPal");
                    if (payPalStandardPaymentSettings.PdtValidateOrderTotal && orderTotalSentToPayPal.HasValue && mc_gross != orderTotalSentToPayPal.Value)
                    {
                        string errorStr = string.Format("PayPal PDT. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal, order.Id);
                        //log
                        _logger.Error(errorStr);
                        //order note
                        order.OrderNotes.Add(new OrderNote
                        {
                            Note = errorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                    //clear attribute
                    if (orderTotalSentToPayPal.HasValue)
                        _genericAttributeService.SaveAttribute<decimal?>(order, "OrderTotalSentToPayPal", null);

                    //mark order as paid
                    if (newPaymentStatus == PaymentStatus.Paid)
                    {
                        if (_orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            order.AuthorizationTransactionId = txn_id;
                            _orderService.UpdateOrder(order);

                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id});
            }
            else
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    //order note
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = "PayPal PDT failed. " + response,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [ValidateInput(false)]
        public ActionResult IPNHandler()
        {
            byte[] param = Request.BinaryRead(Request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);
            Dictionary<string, string> values;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalStandard") as PayPalStandardPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            if (processor.VerifyIpn(strRequest, out values))
            {
                #region values
                decimal mc_gross = decimal.Zero;
                try
                {
                    mc_gross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                }
                catch { }

                string payer_status = string.Empty;
                values.TryGetValue("payer_status", out payer_status);
                string payment_status = string.Empty;
                values.TryGetValue("payment_status", out payment_status);
                string pending_reason = string.Empty;
                values.TryGetValue("pending_reason", out pending_reason);
                string mc_currency = string.Empty;
                values.TryGetValue("mc_currency", out mc_currency);
                string txn_id = string.Empty;
                values.TryGetValue("txn_id", out txn_id);
                string txn_type = string.Empty;
                values.TryGetValue("txn_type", out txn_type);
                string rp_invoice_id = string.Empty;
                values.TryGetValue("rp_invoice_id", out rp_invoice_id);
                string payment_type = string.Empty;
                values.TryGetValue("payment_type", out payment_type);
                string payer_id = string.Empty;
                values.TryGetValue("payer_id", out payer_id);
                string receiver_id = string.Empty;
                values.TryGetValue("receiver_id", out receiver_id);
                string invoice = string.Empty;
                values.TryGetValue("invoice", out invoice);
                string payment_fee = string.Empty;
                values.TryGetValue("payment_fee", out payment_fee);

                #endregion

                var sb = new StringBuilder();
                sb.AppendLine("Paypal IPN:");
                foreach (KeyValuePair<string, string> kvp in values)
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
                }

                var newPaymentStatus = PaypalHelper.GetPaymentStatus(payment_status, pending_reason);
                sb.AppendLine("New payment status: " + newPaymentStatus);

                switch (txn_type)
                {
                    case "recurring_payment_profile_created":
                        //do nothing here
                        break;
                    #region Recurring payment
                    case "recurring_payment":
                        {
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(rp_invoice_id);
                            }
                            catch
                            {
                            }

                            var initialOrder = _orderService.GetOrderByGuid(orderNumberGuid);
                            if (initialOrder != null)
                            {
                                var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: initialOrder.Id);
                                foreach (var rp in recurringPayments)
                                {
                                    switch (newPaymentStatus)
                                    {
                                        case PaymentStatus.Authorized:
                                        case PaymentStatus.Paid:
                                            {
                                                var recurringPaymentHistory = rp.RecurringPaymentHistory;
                                                if (!recurringPaymentHistory.Any())
                                                {
                                                    //first payment
                                                    var rph = new RecurringPaymentHistory
                                                    {
                                                        RecurringPaymentId = rp.Id,
                                                        OrderId = initialOrder.Id,
                                                        CreatedOnUtc = DateTime.UtcNow
                                                    };
                                                    rp.RecurringPaymentHistory.Add(rph);
                                                    _orderService.UpdateRecurringPayment(rp);
                                                }
                                                else
                                                {
                                                    //next payments
                                                    var processPaymentResult = new ProcessPaymentResult();
                                                    processPaymentResult.NewPaymentStatus = newPaymentStatus;
                                                    if (newPaymentStatus == PaymentStatus.Authorized)
                                                        processPaymentResult.AuthorizationTransactionId = txn_id;
                                                    else
                                                        processPaymentResult.CaptureTransactionId = txn_id;

                                                    _orderProcessingService.ProcessNextRecurringPayment(rp, processPaymentResult);
                                                }
                                            }
                                            break;
                                        case PaymentStatus.Voided:
                                            //failed payment
                                            var failedPaymentResult = new ProcessPaymentResult
                                            {
                                                Errors = new[] { string.Format("PayPal IPN. Recurring payment is {0} .", payment_status) },
                                                RecurringPaymentFailed = true
                                            };
                                            _orderProcessingService.ProcessNextRecurringPayment(rp, failedPaymentResult);
                                            break;
                                    }
                                }

                                //this.OrderService.InsertOrderNote(newOrder.OrderId, sb.ToString(), DateTime.UtcNow);
                                _logger.Information("PayPal IPN. Recurring info", new NopException(sb.ToString()));
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new NopException(sb.ToString()));
                            }
                        }                       
                        break;
                    case "recurring_payment_failed":
                        var orderGuid = Guid.Empty;
                        if (Guid.TryParse(rp_invoice_id, out orderGuid))
                        {
                            var initialOrder = _orderService.GetOrderByGuid(orderGuid);
                            if (initialOrder != null)
                            {
                                var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: initialOrder.Id).FirstOrDefault();
                                //failed payment
                                if (recurringPayment != null)
                                    _orderProcessingService.ProcessNextRecurringPayment(recurringPayment, new ProcessPaymentResult { Errors = new[] { txn_type }, RecurringPaymentFailed = true });
                            }
                        }
                        break;
                    #endregion
                    default:
                        #region Standard payment
                        {
                            string orderNumber = string.Empty;
                            values.TryGetValue("custom", out orderNumber);
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(orderNumber);
                            }
                            catch
                            {
                            }

                            var order = _orderService.GetOrderByGuid(orderNumberGuid);
                            if (order != null)
                            {

                                //order note
                                order.OrderNotes.Add(new OrderNote
                                {
                                    Note = sb.ToString(),
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                _orderService.UpdateOrder(order);

                                switch (newPaymentStatus)
                                {
                                    case PaymentStatus.Pending:
                                        {
                                        }
                                        break;
                                    case PaymentStatus.Authorized:
                                        {
                                            //validate order total
                                            if (Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal, 2)))
                                            {
                                                //valid
                                                if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                                                {
                                                    _orderProcessingService.MarkAsAuthorized(order);
                                                }
                                            }
                                            else
                                            {
                                                //not valid
                                                string errorStr = string.Format("PayPal IPN. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal, order.Id);
                                                //log
                                                _logger.Error(errorStr);
                                                //order note
                                                order.OrderNotes.Add(new OrderNote
                                                {
                                                    Note = errorStr,
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow
                                                });
                                                _orderService.UpdateOrder(order);
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Paid:
                                        {
                                            //validate order total
                                            if (Math.Round(mc_gross, 2).Equals(Math.Round(order.OrderTotal, 2)))
                                            {
                                                //valid
                                                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                                {
                                                    order.AuthorizationTransactionId = txn_id;
                                                    _orderService.UpdateOrder(order);

                                                    _orderProcessingService.MarkOrderAsPaid(order);
                                                }
                                            }
                                            else
                                            {
                                                //not valid
                                                string errorStr = string.Format("PayPal IPN. Returned order total {0} doesn't equal order total {1}. Order# {2}.", mc_gross, order.OrderTotal, order.Id);
                                                //log
                                                _logger.Error(errorStr);
                                                //order note
                                                order.OrderNotes.Add(new OrderNote
                                                {
                                                    Note = errorStr,
                                                    DisplayToCustomer = false,
                                                    CreatedOnUtc = DateTime.UtcNow
                                                });
                                                _orderService.UpdateOrder(order);
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Refunded:
                                        {
                                            var totalToRefund = Math.Abs(mc_gross);
                                            if (totalToRefund > 0 && Math.Round(totalToRefund, 2).Equals(Math.Round(order.OrderTotal, 2)))
                                            {
                                                //refund
                                                if (_orderProcessingService.CanRefundOffline(order))
                                                {
                                                    _orderProcessingService.RefundOffline(order);
                                                }
                                            }
                                            else
                                            {
                                                //partial refund
                                                if (_orderProcessingService.CanPartiallyRefundOffline(order, totalToRefund))
                                                {
                                                    _orderProcessingService.PartiallyRefundOffline(order, totalToRefund);
                                                }
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Voided:
                                        {
                                            if (_orderProcessingService.CanVoidOffline(order))
                                            {
                                                _orderProcessingService.VoidOffline(order);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new NopException(sb.ToString()));
                            }
                        }
                        #endregion
                        break;
                }
            }
            else
            {
                _logger.Error("PayPal IPN failed.", new NopException(strRequest));
            }

            //nothing should be rendered to visitor
            return Content("");
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            if (_payPalStandardPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage)
            {
                var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                    customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
                if (order != null)
                {
                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }


        /// <summary>
        /// 根据访问Token获取用户对象
        /// </summary>
        /// <param name="token"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        protected Nop.Core.Domain.Customers.Customer GetCustomerFromToken(string token, string device)
        {
            try
            {
                string text = _encryptionService.DecryptText(token);
                string[] strArr = Regex.Split(text, ":::", RegexOptions.IgnoreCase);
                if (strArr.Length == 2)
                {
                    if (strArr[1].Equals(device, StringComparison.OrdinalIgnoreCase))
                    {
                        int customerId = Convert.ToInt32(strArr[0]);
                        var customer = _customerService.GetCustomerById(customerId);
                        return customer;
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Paypal的Braintree支付使用SDK时需要获取Client Token
        /// </summary>           
        /// <param name="device"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetClientToken(string device)
        {
            try
            {                
                string authorization = _httpContext.Request.Headers["Authorization"];
                var customer = GetCustomerFromToken(authorization, device);
                if (customer != null)
                {
                    var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(_storeContext.CurrentStore.Id);
                    string environment = payPalStandardPaymentSettings.UseSandbox ? "sandbox" : "product";
                    string merchantId = payPalStandardPaymentSettings.MerchantId;
                    string publicKey = payPalStandardPaymentSettings.PublicKey;
                    string privateKey = payPalStandardPaymentSettings.PrivateKey;
                    string merchantAccountId = payPalStandardPaymentSettings.MerchantAccountId;

                    var braintreeGateway = new BraintreeGateway(environment, merchantId, publicKey, privateKey);
                    var clientTokenRequest = new ClientTokenRequest();                    
                    clientTokenRequest.MerchantAccountId = merchantAccountId;
                    string token = braintreeGateway.ClientToken.Generate(clientTokenRequest);
                    //string token = braintreeGateway.ClientToken.Generate();
                    return Json(new { Data = token });
                }
            }
            catch (Exception e) { }            
            return Json(new { Errors = "E_1000" });
        }

        /// <summary>
        /// 确认支付，在Braintree服务端创建支付事务
        /// </summary>
        /// <param name="productId"></param>     
        /// <param name="nonce"></param>  
        /// <param name="device"></param>  
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult CreateTransaction(int productId, string nonce,string device)
        {
            var resp = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var payPalStandardPaymentSettings = _settingService.LoadSetting<PayPalStandardPaymentSettings>(_storeContext.CurrentStore.Id);
                string environment = payPalStandardPaymentSettings.UseSandbox ? "sandbox" : "product";
                string merchantId = payPalStandardPaymentSettings.MerchantId;
                string publicKey = payPalStandardPaymentSettings.PublicKey;
                string privateKey = payPalStandardPaymentSettings.PrivateKey;
                var braintreeGateway = new BraintreeGateway(environment, merchantId, publicKey, privateKey);

                //清空购物车中的数据          
                if (customer.ShoppingCartItems.Count > 0)
                {
                    var itemList = customer.ShoppingCartItems.ToList();
                    foreach (var item in itemList)
                    {
                        _shoppingCartService.DeleteShoppingCartItem(item);
                    }
                }
                //添加当前优惠卷到购物车
                var product = _productService.GetProductById(productId);
                var warnings = _shoppingCartService.AddToCart(customer,
                product, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
                if (warnings.Count > 0)
                {
                    return Json(resp);
                }

                var processPaymentRequest = new ProcessPaymentRequest();
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = "Payments.PayPalStandard";
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    var request = new TransactionRequest
                    {
                        Amount = placeOrderResult.PlacedOrder.OrderTotal,
                        PaymentMethodNonce = nonce,
                        Options = new TransactionOptionsRequest
                        {
                            SubmitForSettlement = true
                        }
                    };

                    Result<Transaction> result = braintreeGateway.Transaction.Sale(request);
                    if (result.IsSuccess())
                    {
                        Transaction transaction = result.Target;
                        resp.ExtraData = transaction.Id;
                        if (_orderProcessingService.CanMarkOrderAsPaid(placeOrderResult.PlacedOrder))
                        {
                            placeOrderResult.PlacedOrder.AuthorizationTransactionId = transaction.Id;
                            //_orderService.UpdateOrder(placeOrderResult.PlacedOrder);

                            _orderProcessingService.MarkOrderAsPaid(placeOrderResult.PlacedOrder);

                            IList dataList = new ArrayList();
                            foreach (var item in placeOrderResult.PlacedOrder.OrderItems)
                            {
                                var coupon = new
                                {
                                    orderId = item.OrderId,
                                    cid = item.CustomOrderItemNumber,
                                    productId = item.Product.Id,
                                    userName = customer.Username,
                                    orderTotal = placeOrderResult.PlacedOrder.OrderTotal,
                                    paymentStatus = placeOrderResult.PlacedOrder.PaymentStatusId,
                                    startTime = placeOrderResult.PlacedOrder.PaidDateUtc.HasValue ? placeOrderResult.PlacedOrder.PaidDateUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                                    endTime = product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                                    deleted = placeOrderResult.PlacedOrder.Deleted
                                };
                                dataList.Add(coupon);
                            }
                            resp.Data = dataList;

                        }
                    }
                    else if (result.Transaction != null)
                    {
                        resp.ExtraData = result.Transaction.Id;                        
                    }
                    else
                    {
                        string errorMessages = "";
                        foreach (ValidationError error in result.Errors.DeepAll())
                        {
                            errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                        }
                        resp.Errors = errorMessages;
                    }

                }
                else
                {
                    resp.Errors = "E_3000";
                }
                               
            }
            else
            {
                resp.Errors = "E_1002";
            }
            return Json(resp);
        }
    }
}