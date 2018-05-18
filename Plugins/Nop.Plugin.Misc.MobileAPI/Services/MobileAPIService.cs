using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Directory;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Core.Caching;
using Nop.Services.Media;
using System.Web;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Plugin.Misc.MobileAPI.Services
{
    public partial class MobileAPIService : IMobileAPIService
    {
        #region Fields
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly HttpContextBase _httpContext;        

        #endregion

        #region Ctor

        public MobileAPIService(ICategoryService categoryService,
            IProductService productService,
            IManufacturerService manufacturerService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            ICacheManager cacheManager,
            IPictureService pictureService,
            HttpContextBase httpContext)
        {
            this._categoryService = categoryService;
            this._productService = productService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Method

        /// <summary>
        /// 根据语言代码查询对应的语言ID
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        public int getLanguageIdByCulture(string culture)
        {
            var availableLanguages = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, "Mobile"), () =>
            {
                var result = _languageService
                    .GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
                    .ToDictionary(lang => lang.LanguageCulture);                    
                return result;
            });

            return availableLanguages[culture]==null? _workContext.WorkingLanguage.Id: availableLanguages[culture].Id;
        }

        #endregion
    }
}
