using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.MobileAPI.Models
{
    public class ConfigurationModel : BaseNopModel
    {              

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.EncryptionKey")]
        public string EncryptionKey { get; set; }
        public bool EncryptionKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.ProductPictureSize")]
        public int ProductPictureSize { get; set; }
        public bool ProductPictureSize_OverrideForStore { get; set; }                

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.QRCodeEncodeMode")]
        public int QRCodeEncodeMode { get; set; }
        public bool QRCodeEncodeMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.QRCodeScale")]
        public int QRCodeScale { get; set; }
        public bool QRCodeScale_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.QRCodeVersion")]
        public int QRCodeVersion { get; set; }
        public bool QRCodeVersion_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.MobileAPI.QRCodeErrorCorrect")]
        public int QRCodeErrorCorrect { get; set; }
        public bool QRCodeErrorCorrect_OverrideForStore { get; set; }
       

    }
}