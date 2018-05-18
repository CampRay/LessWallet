using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Configuration;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Media;
using Nop.Services.Tax;
using Nop.Core.Domain.Stores;
using System.IO;
using System.Xml;
using Nop.Core.Domain.Catalog;
using System.Globalization;

namespace Nop.Plugin.Misc.MobileAPI
{
    public class MobileAPIPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields
        
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly MobileAPISettings _mobileAPISettings;
        private readonly CurrencySettings _currencySettings;
        

        #endregion

        #region Ctor

        public MobileAPIPlugin(IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            ILanguageService languageService,
            ISettingService settingService,
            IWorkContext workContext,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            MobileAPISettings mobileAPISettings,
            CurrencySettings currencySettings)
        {            
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._currencyService = currencyService;
            this._languageService = languageService;
            this._settingService = settingService;
            this._workContext = workContext;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._mobileAPISettings = mobileAPISettings;
            this._currencySettings = currencySettings;            
        }

        #endregion

        #region Utilities
        
       
        #endregion


        #region Methods

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MobileAPI";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.MobileAPI.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new MobileAPISettings
            {
                EncryptionKey= "lesswallet@@2017",
                ProductPictureSize = 125,                
                QRCodeEncodeMode =0,
                QRCodeScale=4,
                QRCodeVersion=8,
                QRCodeErrorCorrect=1
            };
            _settingService.SaveSetting(settings);

            //安装数据data
            //_objectContext.Install();

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.EncryptionKey", "Encryption Key");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.EncryptionKey.Hint", "The encryption private key that will be used to generate the access token.");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.ProductPictureSize", "QR Code thumbnail image size");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.ProductPictureSize.Hint", "The default size (pixels) for the QR Code thumbnail images.");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeEncodeMode", "QR Code Encode Mode");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeEncodeMode.Hint", "The default encode mode that will be used to generate the QR Code.");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeScale", "QR Code scale");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeScale.Hint", "The default pixel size for the data block in the QR Code picture.");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeVersion", "QR Code Version");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeVersion.Hint", "The default version that will be used to generate the QR Code.");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeErrorCorrect", "Error Correct Level");
            this.AddOrUpdatePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeErrorCorrect.Hint", "The default error correct level that will be used to generate the QR Code.");            

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<MobileAPISettings>();

            //删除数据库数据data
            //_objectContext.Uninstall();

            //locales
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.EncryptionKey");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.EncryptionKey.Hint");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.ProductPictureSize");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.ProductPictureSize.Hint");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeEncodeMode");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeEncodeMode.Hint");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeScale");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeScale.Hint");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeVersion");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeVersion.Hint");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeErrorCorrect");
            this.DeletePluginLocaleResource("Plugin.Misc.MobileAPI.QRCodeErrorCorrect.Hint");


            base.Uninstall();
        }

        
        #endregion
    }
}
