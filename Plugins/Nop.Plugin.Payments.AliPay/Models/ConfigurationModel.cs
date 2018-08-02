using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.AliPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Fields.SellerEmail")]
        public string SellerEmail { get; set; }
        public bool SellerEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Fields.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Fields.Partner")]
        public string Partner { get; set; }
        public bool Partner_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.AliPay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}