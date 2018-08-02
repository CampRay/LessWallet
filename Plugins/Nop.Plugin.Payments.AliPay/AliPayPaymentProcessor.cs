using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.AliPay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.AliPay
{
    /// <summary>
    /// AliPay payment processor
    /// </summary>
    public class AliPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly AliPayPaymentSettings _aliPayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private static string GATEWAY_NEW = "https://mapi.alipay.com/gateway.do?";
        //�̻���˽Կ
        private static string _key = "";
        //�����ʽ
        private static string _input_charset = "";
        //ǩ����ʽ
        private static string _sign_type = "";

        #endregion

        #region Ctor

        public AliPayPaymentProcessor(AliPayPaymentSettings aliPayPaymentSettings,
            ISettingService settingService, IWebHelper webHelper,
            IStoreContext storeContext, ILocalizationService localizationService)
        {
            this._aliPayPaymentSettings = aliPayPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._localizationService = localizationService;

            _key = Config.Key.Trim();
            _input_charset = Config.Input_charset.Trim().ToLower();
            _sign_type = Config.Sign_type.Trim().ToUpper();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="Input">Input</param>
        /// <param name="Input_charset">Input charset</param>
        /// <returns>Result</returns>
        public string GetMD5(string Input, string Input_charset)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(Input_charset).GetBytes(Input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Bubble sort
        /// </summary>
        /// <param name="Input">Input</param>
        /// <returns>Result</returns>
        public string[] BubbleSort(string[] Input)
        {
            int i, j;
            string temp;
            bool exchange;

            for (i = 0; i < Input.Length; i++)
            {
                exchange = false;

                for (j = Input.Length - 2; j >= i; j--)
                {
                    if (System.String.CompareOrdinal(Input[j + 1], Input[j]) < 0)
                    {
                        temp = Input[j + 1];
                        Input[j + 1] = Input[j];
                        Input[j] = temp;

                        exchange = true;
                    }
                }

                if (!exchange)
                {
                    break;
                }
            }
            return Input;
        }

        /// <summary>
        /// ��������ʱ��ǩ��
        /// </summary>
        /// <param name="sPara">�����֧�����Ĳ�������</param>.

        /// <returns>ǩ�����</returns>
        private static string BuildRequestMysign(Dictionary<string, string> sPara)
        {
            //����������Ԫ�أ����ա�����=����ֵ����ģʽ�á�&���ַ�ƴ�ӳ��ַ���
            string prestr = Core.CreateLinkString(sPara);

            //�����յ��ַ���ǩ�������ǩ�����
            string mysign = "";

            mysign = AlipayMD5.Sign(prestr, _key, _input_charset);

            return mysign;
        }

        /// <summary>
        /// Create URL
        /// </summary>
        /// <param name="Para">Para</param>
        /// <param name="InputCharset">Input charset</param>
        /// <param name="Key">Key</param>
        /// <returns>Result</returns>
        public string CreatUrl(string[] Para, string InputCharset, string Key)
        {
            int i;
            string[] Sortedstr = BubbleSort(Para);
            StringBuilder prestr = new StringBuilder();

            for (i = 0; i < Sortedstr.Length; i++)
            {
                if (i == Sortedstr.Length - 1)
                {
                    prestr.Append(Sortedstr[i]);

                }
                else
                {
                    prestr.Append(Sortedstr[i] + "&");
                }

            }

            prestr.Append(Key);
            string sign = GetMD5(prestr.ToString(), InputCharset);
            return sign;
        }

        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="StrUrl">Url</param>
        /// <param name="Timeout">Timeout</param>
        /// <returns>Result</returns>
        public string Get_Http(string StrUrl, int Timeout)
        {
            string strResult = string.Empty;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(StrUrl);
                myReq.Timeout = Timeout;
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder strBuilder = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    strBuilder.Append(sr.ReadLine());
                }

                strResult = strBuilder.ToString();
            }
            catch (Exception exc)
            {
                strResult = "Error: " + exc.Message;
            }
            return strResult;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //string gateway = "https://www.alipay.com/cooperate/gateway.do?";
            string service = "create_forex_trade";
            string partner = _aliPayPaymentSettings.Partner;
            string out_trade_no = postProcessPaymentRequest.Order.Id.ToString();
            string subject = "Payment with AliPay";
            string body = "Order from " + _storeContext.CurrentStore.Name;
            string total_fee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);

            string notify_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Notify";
            string return_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentAliPay/Return";

            var sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", _aliPayPaymentSettings.Partner);
            sParaTemp.Add("_input_charset", Config.Input_charset.ToLower());
            sParaTemp.Add("service", "create_forex_trade");
            sParaTemp.Add("return_url", return_url);
            sParaTemp.Add("notify_url", notify_url);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("body", body);
            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("currency", postProcessPaymentRequest.Order.CustomerCurrencyCode); 
            sParaTemp.Add("total_fee", total_fee);

            //BuildRequest(sParaTemp);

            //��ǩ�������������
            var sPara = new Dictionary<string, string>();

            //����ǩ����������
            sPara = Core.FilterPara(sParaTemp);
            string sign = BuildRequestMysign(sPara);

            var post = new RemotePost();
            post.FormName = "alipaysubmit";
            post.Url = "https://mapi.alipay.com/gateway.do?" + "_input_charset=" + _input_charset;
            post.Method = "POST";

            post.Add("partner", partner);
            post.Add("_input_charset", Config.Input_charset.ToLower());
            post.Add("service", service);
            post.Add("return_url", return_url);
            post.Add("notify_url", notify_url);
            post.Add("subject", subject);
            post.Add("body", body);
            post.Add("out_trade_no", out_trade_no);
            post.Add("currency", postProcessPaymentRequest.Order.CustomerCurrencyCode);
            post.Add("total_fee", total_fee);

            post.Add("sign", sign);
            post.Add("sign_type", Config.Sign_type.Trim().ToUpper());

            post.Post();
        }

        /// <summary>
        /// ����������ģ��Զ��HTTP��POST����ʽ���첢��ȡ֧�����Ĵ�����
        /// </summary>
        /// <param name="sParaTemp">�����������</param>
        /// <returns>֧����������</returns>
        public static string BuildRequest(SortedDictionary<string, string> sParaTemp)
        {
            Encoding code = Encoding.GetEncoding(_input_charset);

            //��������������ַ���
            string strRequestData = BuildRequestParaToString(sParaTemp, code);

            //������ת�������������ֽ���������
            byte[] bytesRequestData = code.GetBytes(strRequestData);

            //���������ַ
            string strUrl = GATEWAY_NEW + "_input_charset=" + _input_charset;

            //����Զ��HTTP
            string strResult = "";
            try
            {
                //����HttpWebRequest������Ϣ
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(strUrl);
                myReq.Method = "post";
                myReq.ContentType = "application/x-www-form-urlencoded";

                //���POST����
                myReq.ContentLength = bytesRequestData.Length;
                Stream requestStream = myReq.GetRequestStream();
                requestStream.Write(bytesRequestData, 0, bytesRequestData.Length);
                requestStream.Close();

                //����POST�������������
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();

                //��ȡ������������Ϣ
                StreamReader reader = new StreamReader(myStream, code);
                StringBuilder responseData = new StringBuilder();
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    responseData.Append(line);
                }

                //�ͷ�
                myStream.Close();

                strResult = responseData.ToString();
            }
            catch (Exception exp)
            {
                strResult = "����" + exp.Message;
            }

            return strResult;
        }

        /// <summary>
        /// ����Ҫ�����֧�����Ĳ�������
        /// </summary>
        /// <param name="sParaTemp">����ǰ�Ĳ�������</param>
        /// <returns>Ҫ����Ĳ�������</returns>
        private static Dictionary<string, string> BuildRequestPara(SortedDictionary<string, string> sParaTemp)
        {
            //��ǩ�������������
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            //ǩ�����
            string mysign = "";

            //����ǩ����������
            sPara = Core.FilterPara(sParaTemp);

            //���ǩ�����
            mysign = BuildRequestMysign(sPara);

            //ǩ�������ǩ����ʽ���������ύ��������
            sPara.Add("sign", mysign);
            sPara.Add("sign_type", _sign_type);

            return sPara;
        }




        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentAliPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentAliPay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAliPayController);
        }

        public override void Install()
        {
            //settings
            var settings = new AliPayPaymentSettings()
            {
                UseSandbox = true,
                SellerEmail = "",
                Key = "",
                Partner = "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.RedirectionTip", "You will be redirected to AliPay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.SellerEmail", "Seller email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.SellerEmail.Hint", "Enter seller email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.Key.Hint", "Enter key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.Partner", "Partner");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.Partner.Hint", "Enter partner.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.AliPay.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.SellerEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.SellerEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.Partner");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.Partner.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.AliPay.RedirectionTip");

            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            //����true����ʾ���û�ѡ���֧�������󣬽�������ʾPaymentInfo.cshtmlҳ�沿�֣�ֱ�ӽ��붩��ȷ��ҳ��
            get { return true; }
        }

        public string PaymentMethodDescription
        {
            get
            {
                return _localizationService.GetResource("Plugins.Payments.AliPay.PaymentMethodDescription");
            }
        }


        /// <summary>
        /// ����Ҫ�����֧�����Ĳ�������
        /// </summary>
        /// <param name="sParaTemp">����ǰ�Ĳ�������</param>
        /// <param name="code">�ַ�����</param>
        /// <returns>Ҫ����Ĳ��������ַ���</returns>
        private static string BuildRequestParaToString(SortedDictionary<string, string> sParaTemp, Encoding code)
        {
            //��ǩ�������������
            Dictionary<string, string> sPara = new Dictionary<string, string>();
            sPara = BuildRequestPara(sParaTemp);

            //�Ѳ�����������Ԫ�أ����ա�����=����ֵ����ģʽ�á�&���ַ�ƴ�ӳ��ַ��������Բ���ֵ��urlencode
            string strRequestData = Core.CreateLinkStringUrlencode(sPara, code);

            return strRequestData;
        }
        #endregion
    }
}
