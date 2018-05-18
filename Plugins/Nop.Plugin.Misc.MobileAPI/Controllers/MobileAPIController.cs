using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Stores;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.MobileAPI.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Security;
using Nop.Plugin.Misc.MobileAPI.Models;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.Customer;
using Nop.Services.Authentication;
using Nop.Services.Events;
using System.Web.Security;

namespace Nop.Plugin.Misc.MobileAPI.Controllers
{
    [AdminAuthorize]
    public class MobileAPIController : BasePluginController
    {
        private readonly IMobileAPIService _mobileAPIService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IStoreService _storeService;
        private readonly MobileAPISettings _mobileAPISettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly HttpContextBase _httpContext;
        private readonly IWorkContext _workContext;

        public MobileAPIController(IMobileAPIService mobileAPIService,
            IProductService productService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPluginFinder pluginFinder,
            ILogger logger,
            IWebHelper webHelper,
            IStoreService storeService,
            MobileAPISettings mobileAPISettings,
            ISettingService settingService,
            IPermissionService permissionService,
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            HttpContextBase httpContext,
            IWorkContext workContext)
        {
            this._mobileAPIService = mobileAPIService;
            this._productService = productService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._pluginFinder = pluginFinder;
            this._logger = logger;
            this._webHelper = webHelper;
            this._storeService = storeService;
            this._mobileAPISettings = mobileAPISettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._customerRegistrationService = customerRegistrationService;
            this._customerSettings = customerSettings;
            this._authenticationService = authenticationService;
            this._customerActivityService = customerActivityService;
            this._eventPublisher = eventPublisher;
            this._httpContext = httpContext;
            this._workContext = workContext;
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mobileAPISettings = _settingService.LoadSetting<MobileAPISettings>(storeScope);

            var model = new ConfigurationModel();
            model.EncryptionKey = mobileAPISettings.EncryptionKey;
            model.ProductPictureSize = mobileAPISettings.ProductPictureSize;                                                
            model.QRCodeEncodeMode = mobileAPISettings.QRCodeEncodeMode;
            model.QRCodeScale = mobileAPISettings.QRCodeScale;
            model.QRCodeVersion = mobileAPISettings.QRCodeVersion;
            model.QRCodeErrorCorrect = mobileAPISettings.QRCodeErrorCorrect;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.EncryptionKey_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.EncryptionKey, storeScope);
                model.ProductPictureSize_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.ProductPictureSize, storeScope);
                model.QRCodeEncodeMode_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.QRCodeEncodeMode, storeScope);
                model.QRCodeScale_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.QRCodeScale, storeScope);
                model.QRCodeVersion_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.QRCodeVersion, storeScope);
                model.QRCodeErrorCorrect_OverrideForStore = _settingService.SettingExists(mobileAPISettings, x => x.QRCodeErrorCorrect, storeScope);                             
            }

            return View("~/Plugins/Misc.MobileAPI/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        [FormValueRequired("save")]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mobileAPISettings = _settingService.LoadSetting<MobileAPISettings>(storeScope);

            //save settings
            mobileAPISettings.EncryptionKey = model.EncryptionKey;
            mobileAPISettings.ProductPictureSize = model.ProductPictureSize;
            mobileAPISettings.QRCodeEncodeMode = model.QRCodeEncodeMode;
            mobileAPISettings.QRCodeScale = model.QRCodeScale;
            mobileAPISettings.QRCodeVersion = model.QRCodeVersion;
            mobileAPISettings.QRCodeErrorCorrect = model.QRCodeErrorCorrect;


            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.EncryptionKey, model.EncryptionKey_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.ProductPictureSize, model.ProductPictureSize_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.QRCodeEncodeMode, model.QRCodeEncodeMode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.QRCodeScale, model.QRCodeScale_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.QRCodeVersion, model.QRCodeVersion_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(mobileAPISettings, x => x.QRCodeErrorCorrect, model.QRCodeErrorCorrect_OverrideForStore, storeScope, false);
            
            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        
    }
}
