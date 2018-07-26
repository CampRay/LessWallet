using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.MobileAPI.Services;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using System.Xml.Serialization;
using static Nop.Services.Payments.PaymentExtensions;

namespace Nop.Plugin.Misc.MobileAPI.Controllers
{
    public partial class UserAPIController : BasePublicController
    {
        private readonly IMobileAPIService _mobileAPIService;
        private readonly IProductService _productService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
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
        private readonly IStoreContext _storeContext;
        private readonly IEncryptionService _encryptionService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILanguageService _languageService;
        private readonly ICacheManager _cacheManager;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreMappingService _storeMappingService;

        public UserAPIController(IMobileAPIService mobileAPIService,
            IProductService productService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
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
            IWorkContext workContext,
            IStoreContext storeContext,
            IEncryptionService encryptionService,
            IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IShoppingCartService shoppingCartService,
            IOrderService orderService,
            IAddressService addressService,
            ICountryService countryService,
            ILanguageService languageService,
            ICacheManager cacheManager,
            IPictureService pictureService,
            IVendorService vendorService,
            ISpecificationAttributeService specificationAttributeService,
            IProductAttributeService productAttributeService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService, IPriceFormatter priceFormatter,
            IRewardPointService rewardPointService,
            IStoreMappingService storeMappingService)
        {
            this._mobileAPIService = mobileAPIService;
            this._productService = productService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
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
            this._storeContext = storeContext;
            this._encryptionService = encryptionService;
            this._genericAttributeService = genericAttributeService;
            this._orderProcessingService = orderProcessingService;
            this._shoppingCartService = shoppingCartService;
            this._orderService = orderService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._languageService = languageService;
            this._cacheManager = cacheManager;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._vendorService = vendorService;
            this._productAttributeService = productAttributeService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._priceFormatter = priceFormatter;
            this._rewardPointService = rewardPointService;
            this._storeMappingService = storeMappingService;
        }


        #region Utilities

        /// <summary>
        /// 验证访问Token是否与手机ID一致
        /// </summary>
        /// <param name="token"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool validateToken(string token,string device)
        {
            string text = _encryptionService.DecryptText(token, _mobileAPISettings.EncryptionKey);
            string[] strArr = text.Split(';');
            if (strArr.Length == 2)
            {
                if (strArr[1].Equals(device, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据访问Token获取用户对象
        /// </summary>
        /// <param name="token"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private Customer GetCustomerFromToken(string token, string device)
        {
            try
            {
                string text = _encryptionService.DecryptText(token, _mobileAPISettings.EncryptionKey);
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
        /// Prepare the product specification models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>List of product specification model</returns>
        private IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return _cacheManager.Get(cacheKey, () =>
                _specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                .Select(psa =>
                {
                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = psa.SpecificationAttributeOption.GetLocalized(x => x.Name),
                        ColorSquaresRgb = psa.SpecificationAttributeOption.ColorSquaresRgb
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option://可选项
                            m.ValueRaw = m.SpecificationAttributeName;
                            //设置ValueRaw为自定义的值，修改选项没有此值
                            //m.ValueRaw = HttpUtility.HtmlEncode(psa.SpecificationAttributeOption.GetLocalized(x => x.Name));
                            if (m.SpecificationAttributeId == 6 || m.SpecificationAttributeId == 7 || m.SpecificationAttributeId == 8)
                            {
                                //对于相应的图片属性选项，则生成图片URL
                                m.ColorSquaresRgb = _pictureService.GetPictureUrl(Convert.ToInt32(m.ColorSquaresRgb), 150, false);
                                m.ValueRaw = m.ColorSquaresRgb;
                            }
                            else if(m.SpecificationAttributeId == 5)//颜色选项
                            {
                                m.ValueRaw = m.ColorSquaresRgb;
                            }                            
                            break;
                        case SpecificationAttributeType.CustomText://自定义文本
                            m.ValueRaw = HttpUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText://自定义html文本
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink://自定义URL文本
                            m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", psa.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomColor://自定义颜色值
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case SpecificationAttributeType.CustomImage://自定义图片
                            if (m.SpecificationAttributeId == 6 || m.SpecificationAttributeId == 7 || m.SpecificationAttributeId == 8)
                            {
                                m.ValueRaw = _pictureService.GetPictureUrl(Convert.ToInt32(psa.CustomValue), 150, false);
                            }
                            else
                            {
                                m.ValueRaw = psa.CustomValue;
                            }
                            break;
                        default:
                            break;
                    }
                    return m;
                }).ToList()
            );
        }


        /// <summary>
        /// 准备商品属性模型
        /// </summary>
        /// <param name="product">Product</param>        
        /// <returns>List of product attribute model</returns>
        protected virtual IList<ProductDetailsModel.ProductAttributeModel> PrepareProductAttributeModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //performance optimization
            //We cache a value indicating whether a product has attributes
            IList<ProductAttributeMapping> productAttributeMapping = null;
            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_HAS_PRODUCT_ATTRIBUTES_KEY, product.Id);
            var hasProductAttributesCache = _cacheManager.Get<bool?>(cacheKey);
            if (!hasProductAttributesCache.HasValue)
            {
                //no value in the cache yet
                //let's load attributes and cache the result (true/false)
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                hasProductAttributesCache = productAttributeMapping.Any();
                _cacheManager.Set(cacheKey, hasProductAttributesCache, 60);
            }
            if (hasProductAttributesCache.Value && productAttributeMapping == null)
            {
                //cache indicates that the product has attributes
                //let's load them
                productAttributeMapping = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            }
            if (productAttributeMapping == null)
            {
                productAttributeMapping = new List<ProductAttributeMapping>();
            }

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            foreach (var attribute in productAttributeMapping)
            {
                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                    Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue,
                    HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel 
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal taxRate;
                            decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                            decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                            else if (priceAdjustmentBase < decimal.Zero)
                                valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        ////"image square" picture (with with "image squares" attribute type only)
                        //if (attributeValue.ImageSquaresPictureId > 0)
                        //{
                        //    var productAttributeImageSquarePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_MODEL_KEY,
                        //           attributeValue.ImageSquaresPictureId,
                        //           _webHelper.IsCurrentConnectionSecured(),
                        //           _storeContext.CurrentStore.Id);
                        //    valueModel.ImageSquaresPictureModel = _cacheManager.Get(productAttributeImageSquarePictureCacheKey, () =>
                        //    {
                        //        var imageSquaresPicture = _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
                        //        if (imageSquaresPicture != null)
                        //        {
                        //            return new PictureModel
                        //            {
                        //                FullSizeImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture),
                        //                ImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize)
                        //            };
                        //        }
                        //        return new PictureModel();
                        //    });
                        //}

                        //picture of a product attribute value
                        valueModel.PictureId = attributeValue.PictureId;
                    }

                }

                

                model.Add(attributeModel);
            }

            return model;
        }


        /// <summary>
        /// 准备Coupon订单用户属性模型
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected product attribute mapping identifiers</returns>
        protected virtual Dictionary<int,string> ParseProductAttributeMappingIds(string attributesXml)
        {
            var values = new Dictionary<int, string>();
            if (String.IsNullOrEmpty(attributesXml))
                return null;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        int id;
                        if (int.TryParse(str1, out id))
                        {
                            string value = node1.FirstChild.FirstChild.Value;
                            values.Add(id,value);
                        }
                        
                    }
                    
                }
            }
            catch (Exception exc)
            {
                return null;
            }
            return values;
        }

        #endregion



        #region Method

        /// <summary>
        /// 用户注册API
        /// </summary>
        /// <param name="device">识别移动设备的唯一ID</param>
        /// <param name="uname"></param>
        /// <param name="email"></param>
        /// <param name="pwd"></param>
        /// <param name="country"></param>
        /// <param name="vender">如果是商家的APP申请登录，则此字段必须设置为true</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult Register(string device, string uname, string email, string pwd, string firstname, string lastname, string address, string countrycode, bool vender = false)
        {
            if (_workContext.CurrentCustomer.IsRegistered())
            {
                //Already registered customer. 
                _authenticationService.SignOut();                
                //Save a new record
                _workContext.CurrentCustomer = _customerService.InsertGuestCustomer();
            }
            var customer = _workContext.CurrentCustomer;
            bool isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
            var registrationRequest = new CustomerRegistrationRequest(_workContext.CurrentCustomer,
                email, uname, pwd,_customerSettings.DefaultPasswordFormat,_storeContext.CurrentStore.Id,isApproved);
            var registrationResult = _customerRegistrationService.RegisterCustomer(registrationRequest);
            if (registrationResult.Success)
            {
                var countryObj = _countryService.GetCountryByTwoLetterIsoCode(countrycode);
                int countryId = countryObj == null ? 0 : countryObj.Id;
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, firstname);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastName, lastname);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.StreetAddress, address);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, uname);
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CountryId, countryId);
                //insert default address (if possible)
                var defaultAddress = new Address
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    Company = null,
                    CountryId = countryId,
                    StateProvinceId = null,
                    City = null,
                    Address1 = address,
                    Address2 = null,
                    ZipPostalCode = null,
                    PhoneNumber = uname,
                    FaxNumber = null,
                    CreatedOnUtc = customer.CreatedOnUtc
                };
                if (this._addressService.IsAddressValid(defaultAddress))
                {
                    //some validation
                    if (defaultAddress.CountryId == 0)
                        defaultAddress.CountryId = null;
                    if (defaultAddress.StateProvinceId == 0)
                        defaultAddress.StateProvinceId = null;
                    //set default address
                    customer.Addresses.Add(defaultAddress);
                    customer.BillingAddress = defaultAddress;
                    customer.ShippingAddress = defaultAddress;
                    _customerService.UpdateCustomer(customer);
                }
                //如果注册的是商家账号，添加商家角色
                if (vender) { 
                    var vendorRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Vendors);
                    customer.CustomerRoles.Add(vendorRole);
                    _customerService.UpdateCustomer(customer);
                }

                return Login(device, uname, pwd, vender);
            }
            else
            {                
                var result = new DataSourceResult
                {
                    Errors = registrationResult.Errors[0],                    
                };

                return Json(result);
            }            
        }

        /// <summary>
        /// 用户登录API
        /// </summary>
        /// <param name="device">识别移动设备的唯一ID</param>
        /// <param name="uname"></param>
        /// <param name="pwd"></param>
        /// <param name="vender">如果是商家的APP申请登录，则此字段必须设置为true</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult Login(string device,string uname, string pwd, bool vender=false)
        {
            string errors = null;            
            IList dataList = new ArrayList();            
            try
            {
                var loginResult =
                    _customerRegistrationService.ValidateCustomer(uname.Trim(), pwd);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerService.GetCustomerByUsernameOrEmail(uname);
                            if (customer == null)
                            {
                                errors = "E_2000";
                                break;
                            }
                            //如果申请登录的是Vender，但客户角色不是Vender，则判定登录失败
                            Vendor vendor = new Vendor();
                            if (vender)
                            {
                                if (!customer.IsVendor())
                                {
                                    vendor = _vendorService.GetVendorById(customer.VendorId);
                                }
                                else
                                {
                                    errors = "E_2000";
                                    break;
                                }
                                
                            }
                            //生成移动端的访问Token
                            string text = customer.Id + ":::" + device;
                            string encryptedToken = _encryptionService.EncryptText(text, _mobileAPISettings.EncryptionKey);

                            Dictionary<int, int> storePoints = new Dictionary<int, int>();
                            IList<Store> storeList = _storeService.GetAllStores();
                            foreach (var item in storeList)
                            {
                                int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(customer.Id, item.Id);
                                storePoints.Add(item.Id, rewardPointsBalance);
                            }
                            string avatarUrl = _pictureService.GetPictureUrl(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                            //int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(_workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);                            
                            
                            var customerData = new
                            {
                                id= customer.Id,
                                userName = customer.Username,
                                email = customer.Email,
                                mobile = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                                firstName = customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                                lastName = customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                                birthday = "",
                                countryId = customer.BillingAddress.CountryId,
                                address = customer.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                                token = encryptedToken,                                
                                avatarUrl = avatarUrl,
                                storePoints = storePoints,
                                venderId= customer.VendorId,
                                merchant = new
                                {
                                    id = vendor.Id,
                                    name = vendor.Name,
                                    desc = vendor.Description,
                                    storeId = vendor.StoreId,
                                    pictureUrl = _pictureService.GetPictureUrl(vendor.PictureId, 200),
                                }
                            };                                                        

                            dataList.Add(customerData);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity(customer, "PublicStore.Login", _localizationService.GetResource("ActivityLog.PublicStore.Login"));
                            break;
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        errors = "E_2000";
                        break;
                    case CustomerLoginResults.Deleted:
                        errors = "E_2002";
                        break;
                    case CustomerLoginResults.NotActive:
                        errors = "E_2001";
                        break;
                    case CustomerLoginResults.NotRegistered:
                        errors = "E_2000";
                        break;
                    case CustomerLoginResults.LockedOut:
                        errors = "E_2003";
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        errors = "E_2000";
                        break;
                }

            }
            catch(Exception exe)
            {
                errors = "E_1000";
            }
            var result = new DataSourceResult
            {
                Errors = errors,                
                Data= dataList
            };

            return Json(result);
        }

        /// <summary>
        /// 修改用户信息API
        /// </summary>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult EditUser(FormCollection form)
        {
            string errors = null;                    
            string authorization = _httpContext.Request.Headers["Authorization"];
            string device = form["device"];
            var customer=GetCustomerFromToken(authorization, device);
            if (customer!=null)
            {
                string pwd = form["pwd"];
                string email = form["email"];
                //修改密码
                if (!string.IsNullOrEmpty(pwd))
                {
                    var customerPassword = _customerService.GetCurrentPassword(customer.Id);
                    switch (customerPassword.PasswordFormat)
                    {
                        case PasswordFormat.Clear:
                            customerPassword.Password = pwd;
                            break;
                        case PasswordFormat.Encrypted:
                            customerPassword.Password = _encryptionService.EncryptText(pwd);
                            break;
                        case PasswordFormat.Hashed:
                            {
                                var saltKey = _encryptionService.CreateSaltKey(5);
                                customerPassword.PasswordSalt = saltKey;
                                customerPassword.Password = _encryptionService.CreatePasswordHash(pwd, saltKey, _customerSettings.HashedPasswordFormat);
                            }
                            break;
                    }
                    customerPassword.CreatedOnUtc= DateTime.UtcNow;
                    _customerService.UpdateCustomerPassword(customerPassword);
                }
                //修改电邮
                if (!string.IsNullOrEmpty(email))
                {
                    customer.Email = email;
                    _customerService.UpdateCustomer(customer);
                }


            }
            else
            {
                errors = "E_1000";
            }

            var result = new DataSourceResult
            {
                Errors = errors                
            };

            return Json(result);
        }
        

        /// <summary>
        /// 根据用户ID查询用户信息
        /// </summary>
        /// <param name="id">用户ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GeUserById(int id, string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var user = _customerService.GetCustomerById(id);
                IList dataList = new ArrayList();
                if (user != null)
                {
                    Dictionary<int, int> storePoints = new Dictionary<int, int>();
                    IList<Store> storeList = _storeService.GetAllStores();
                    foreach (var item in storeList)
                    {
                        int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(user.Id, item.Id);
                        storePoints.Add(item.Id, rewardPointsBalance);
                    }
                    //int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(id, _storeContext.CurrentStore.Id);
                    string avatarUrl = _pictureService.GetPictureUrl(user.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                    var userData = new
                    {
                        id = user.Id,
                        userName = user.Username,
                        email = user.Email,
                        mobile = user.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        firstName = user.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        lastName = user.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                        birthday = "",
                        countryId = user.BillingAddress.CountryId,
                        address = user.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),                        
                        avatarUrl = avatarUrl,
                        storePoints = storePoints
                    };
                    dataList.Add(userData);
                    result.Data = dataList;
                }
                else
                {
                    result.Errors = "E_3000";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }        

        /// <summary>
        /// 根据商家ID查询商家信息
        /// </summary>
        /// <param name="id">商家ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetMerchant(int id, string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var vendor = _vendorService.GetVendorById(id);
                IList dataList = new ArrayList();
                if (vendor != null)
                {                    
                    var merchant = new
                    {
                        id = vendor.Id,
                        name = vendor.Name,
                        desc = vendor.Description,
                        storeId = vendor.StoreId,
                        pictureUrl = _pictureService.GetPictureUrl(vendor.PictureId,200),                         
                    };
                    dataList.Add(merchant);
                    result.Data = dataList;
                }
                else
                {
                    result.Errors = "E_3000";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }

        /// <summary>
        /// 根据卡卷商品ID查询获取单个卡卷商品信息API
        /// </summary>
        /// <param name="productId">电子卷ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetProduct(int productId,string device)
        {
            var result = new DataSourceResult();            
            string authorization = _httpContext.Request.Headers["Authorization"];
            //不能用_workContext.CurrentCustomer获取此用户，_workContext.CurrentCustomer只能获取临时用户            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {                
                //查询商品信息
                var product=_productService.GetProductById(productId);   
                ////如果商品已移除或未发布或没库存
                //if(product.Deleted|| !product.Published|| product.StockQuantity<=0)
                //{
                //    result.Errors = "E_3000";
                //    return Json(result);
                //}          
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor == null)
                {
                    vendor = new Vendor();
                }
                IList dataList = new ArrayList();                
                var couponProduct = new
                {
                    productId = product.Id,
                    productTypeId = product.ProductTypeId,
                    productTemplateId = product.ProductTemplateId,
                    title = product.Name,
                    shortDesc = product.ShortDescription,
                    fullDesc = product.FullDescription,
                    agreement = product.UserAgreementText,
                    numPrefix = product.Sku,//卡卷编号前缀
                    price = product.Price,
                    currencyCode = _workContext.WorkingCurrency.CurrencyCode,
                    startTime = product.AvailableStartDateTimeUtc.HasValue ? product.AvailableStartDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    specAttr = PrepareProductSpecificationModel(product),
                    userAttr = PrepareProductAttributeModels(product),
                    merchantId = product.VendorId,                         
                    published = product.Published,
                    deleted=product.Deleted,
                    stockQuantity = product.StockQuantity,
                    merchant = new
                    {
                        id = vendor.Id,
                        name = vendor.Name,
                        desc = vendor.Description,                        
                        storeId = vendor.StoreId,
                        pictureUrl = _pictureService.GetPictureUrl(vendor.PictureId, 200),
                    },

                };
                dataList.Add(couponProduct);
                result.Data = dataList;                                        
                
            }
            else
            {
                result.Errors = "E_1002";
            }            
            return Json(result);
        }       

        /// <summary>
        /// 确认购买或获取单个电子卷API
        /// </summary>
        /// <param name="productId">电子卷ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult CreateOrder(int productId, string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            //不能用_workContext.CurrentCustomer获取此用户，_workContext.CurrentCustomer只能获取临时用户            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
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
                    return Json(result);
                }

                var processPaymentRequest = new ProcessPaymentRequest();
                processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                processPaymentRequest.CustomerId = customer.Id;                
                processPaymentRequest.PaymentMethodSystemName = customer.GetAttribute<string>(
                    SystemCustomerAttributeNames.SelectedPaymentMethod,
                    _genericAttributeService, _storeContext.CurrentStore.Id);
                //保存Coupon的用户权益规则
                var attrList = PrepareProductSpecificationModel(product);
                foreach (var attr in attrList)
                {
                    if (attr.SpecificationAttributeId >= 1 && attr.SpecificationAttributeId <4)
                    {
                        processPaymentRequest.CustomValues.Add(attr.SpecificationAttributeId.ToString(), attr.ValueRaw);
                    }
                    else if (attr.SpecificationAttributeId == 4)
                    {
                        processPaymentRequest.CustomValues.Add(attr.SpecificationAttributeId.ToString(), "0");
                    }
                }
                
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    IList dataList = new ArrayList();

                    foreach (var item in placeOrderResult.PlacedOrder.OrderItems)
                    {
                        var coupon = new
                        {
                            orderId = item.OrderId,
                            cid = placeOrderResult.PlacedOrder.CustomOrderNumber,
                            productId = item.Product.Id,
                            userId = placeOrderResult.PlacedOrder.CustomerId,
                            orderTotal = placeOrderResult.PlacedOrder.OrderTotal,
                            paymentStatus = placeOrderResult.PlacedOrder.PaymentStatusId,
                            startTime= placeOrderResult.PlacedOrder.PaidDateUtc.HasValue? placeOrderResult.PlacedOrder.PaidDateUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                            //endTime = "",
                            deleted = placeOrderResult.PlacedOrder.Deleted,
                            customValues = placeOrderResult.PlacedOrder.DeserializeCustomValues()
                        };
                        dataList.Add(coupon);
                    }                                        
                    result.Data = dataList;
                }
                else
                {
                    
                    result.Errors = "E_3000";
                }
            }
            else
            {                
                result.Errors = "E_1002";
            }
            return Json(result);
        }
        

        /// <summary>
        /// 查询用户已有的单个电子卷API
        /// </summary>
        /// <param name="orderId">订单ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetOrder(int orderId,string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var order = _orderService.GetOrderById(orderId);
                if (order!=null&&order.PaymentStatus== PaymentStatus.Paid)
                {
                    var itme = order.OrderItems.FirstOrDefault();
                    var product = order.OrderItems.FirstOrDefault().Product;                    
                    IList dataList = new ArrayList();
                    var coupon = new
                    {
                        orderId = itme.OrderId,
                        cid = order.CustomOrderNumber,
                        productId = product.Id,
                        userId = order.CustomerId,
                        orderTotal = order.OrderTotal,
                        paymentStatus = order.PaymentStatusId,
                        startTime = order.PaidDateUtc.HasValue ? order.PaidDateUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                        //endTime = "",
                        deleted = order.Deleted,
                        customValues = order.DeserializeCustomValues()
                        //values = ParseProductAttributeMappingIds(itme.AttributesXml)//用户选择或输入属性值集合
                    };
                    dataList.Add(coupon);
                    result.Data = dataList;
                }
                else
                {
                    result.Errors = "E_3000";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }

        /// <summary>
        /// 获取用户所有已有的电子卷API（新安装运行APP时调用）
        /// </summary>         
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllOrders(string device)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();
                var orderStatusIds = new List<int>() { (int)OrderStatus.Complete };
                var paymentStatusIds = new List<int>() { (int)PaymentStatus.Paid};               

                var orders = _orderService.SearchOrders(
                    customerId: customer.Id,                    
                    osIds: orderStatusIds,
                    psIds: paymentStatusIds);
                foreach (var order in orders)
                {
                    var itme = order.OrderItems.FirstOrDefault();
                    var product = order.OrderItems.FirstOrDefault().Product;
                    var vender = _vendorService.GetVendorById(product.VendorId);
                    var coupon = new
                    {
                        orderId = order.Id,
                        cid = order.CustomOrderNumber,
                        productId = product.Id,
                        userId = order.CustomerId,
                        orderTotal = order.OrderTotal,
                        paymentStatus = order.PaymentStatusId,
                        startTime = order.PaidDateUtc.HasValue ? order.PaidDateUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                        //endTime = "",
                        deleted = order.Deleted,
                        customValues = order.DeserializeCustomValues()
                        //values = ParseProductAttributeMappingIds(itme.AttributesXml)//用户选择或输入属性值集合
                    };
                    dataList.Add(coupon);
                }
                result.Data = dataList;
                result.Total = dataList.Count;
            }
            else
            {
                result.Errors = "E_1002";
            }            

            return Json(result);
        }

        /// <summary>
        /// 删除电子卷(未使用情况下删除)
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult DelOrders(string device, string orderIds,bool isvendor)
        {
            var result = new DataSourceResult();            
            string authorization = _httpContext.Request.Headers["Authorization"];            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                if (!string.IsNullOrEmpty(orderIds)) {
                    var ids=orderIds.Split(',');
                    if (ids != null && ids.Count() > 0)
                    {
                        try
                        {
                            IList dataList = new ArrayList();
                            foreach (var idstr in ids)
                            {
                                int id=Convert.ToInt32(idstr);
                                var order = _orderService.GetOrderById(id);
                                if (order != null)
                                {
                                    _orderService.DeleteOrder(order);
                                    if (isvendor)
                                    {                                        
                                        //添加商户收回电子卷的活动日志
                                        _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Coupon.Redeemed", order.Customer.Id+":"+id.ToString());
                                        _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Coupon.Used", customer.Id + ":" + id.ToString());
                                    }                                    
                                    dataList.Add(id);
                                }
                            }
                            result.Data = dataList;
                        }
                        catch (Exception exe)
                        {
                            result.Errors = "E_3001";
                        }

                    }
                }
                else
                {
                    result.Errors = "E_1002";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }           

            return Json(result);
        }
                

        /// <summary>
        /// 根据名称或Email搜索用户
        /// </summary>         
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult SearchUsers(string device,string search)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();               
                var user = _customerService.GetCustomerBySearch(search);
                if (user!=null)
                {                                        
                    string avatarUrl = _pictureService.GetPictureUrl(user.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                    var customerData = new
                    {
                        id = user.Id,
                        userName = user.Username,
                        email = user.Email,
                        mobile = user.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        firstName = user.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        lastName = user.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                        birthday = "",
                        countryId = user.BillingAddress.CountryId,
                        address = user.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                        avatarUrl= avatarUrl                        
                    };

                    dataList.Add(customerData);                    
                }
                result.Data = dataList;
                result.Total = dataList.Count;

            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 搜索用户所有好友
        /// </summary>         
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllFriends(string device)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();                
                var friendsStr= customer.GetAttribute<string>("Friends");
                if (!string.IsNullOrEmpty(friendsStr))
                {
                    var friends= friendsStr.Split(',');
                    foreach (var friendId in friends)
                    {
                        var friend = _customerService.GetCustomerById(Convert.ToInt32(friendId));
                        if (friend != null)
                        {
                            string avatarUrl = _pictureService.GetPictureUrl(friend.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                            var friendData = new
                            {
                                id = friend.Id,
                                userName = friend.Username,
                                email = friend.Email,
                                mobile = friend.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                                firstName = friend.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                                lastName = friend.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                                birthday = "",
                                countryId = friend.BillingAddress.CountryId,
                                address = friend.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                                avatarUrl = avatarUrl
                            };
                            dataList.Add(friendData);
                        }
                    }                    
                }
                result.Data = dataList;
                result.Total = dataList.Count;

            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }


        /// <summary>
        /// 添加好友
        /// </summary>         
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult AddFriend(string device, string username)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();
                var orderStatusIds = new List<int>() { (int)OrderStatus.Complete };
                var paymentStatusIds = new List<int>() { (int)PaymentStatus.Paid };

                var friend = _customerService.GetCustomerByUsernameOrEmail(username);
                if (friend != null)
                {
                    var friendsStr=customer.GetAttribute<string>("Friends");
                    if (string.IsNullOrEmpty(friendsStr))
                    {
                        _genericAttributeService.SaveAttribute(customer, "Friends", friend.Id+"");
                    }
                    else
                    {
                        string[] friendArr = friendsStr.Split(',');
                        if (!friendArr.Contains(friend.Id + ""))
                        {
                            var friends = friendArr.ToList();
                            friends.Add(friend.Id + "");
                            _genericAttributeService.SaveAttribute(customer, "Friends", string.Join(",", friends));
                        }                        
                    }

                    string avatarUrl = _pictureService.GetPictureUrl(friend.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                    var friendData = new
                    {
                        id = friend.Id,
                        userName = friend.Username,
                        email = friend.Email,
                        mobile = friend.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                        firstName = friend.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                        lastName = friend.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                        birthday = "",
                        countryId = friend.BillingAddress.CountryId,
                        address = friend.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                        avatarUrl = avatarUrl
                    };
                    dataList.Add(friendData);                    
                }
                result.Data = dataList;
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 删除好友
        /// </summary>         
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult DelFriend(string device, string username)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {                
                var orderStatusIds = new List<int>() { (int)OrderStatus.Complete };
                var paymentStatusIds = new List<int>() { (int)PaymentStatus.Paid };

                var friend = _customerService.GetCustomerByUsernameOrEmail(username);
                if (friend != null)
                {
                    var friendsStr = customer.GetAttribute<string>("Friends");
                    if (!string.IsNullOrEmpty(friendsStr))
                    {
                        string[] friendArr = friendsStr.Split(',');
                        if (friendArr.Contains(friend.Id + ""))
                        {
                            var friends = friendArr.ToList();
                            friends.Remove(friend.Id + "");
                            _genericAttributeService.SaveAttribute(customer, "Friends", string.Join(",", friends));
                        }
                    }
                }
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }


        /// <summary>
        /// 查询国家表数据API
        /// </summary>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllCountries()
        {
            var result = new DataSourceResult();
            try{
                IList dataList = new ArrayList();
                var countries = _countryService.GetAllCountries();
                foreach (var item in countries)
                {
                    var country = new
                    {
                        id = item.Id,
                        name = item.Name,
                        twoLetterIsoCode = item.TwoLetterIsoCode,
                        threeLetterIsoCode = item.ThreeLetterIsoCode,
                        displayOrder = item.DisplayOrder                        
                    };
                    dataList.Add(country);                    
                }
                result.Data = dataList;
            }
            catch
            {
                result.Errors = "E_1003";
            }
            return Json(result);
        }

        /// <summary>
        /// 查询语言表数据API
        /// </summary>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllLanguages()
        {
            var result = new DataSourceResult();
            try
            {
                IList dataList = new ArrayList();
                var languages = _languageService.GetAllLanguages();
                foreach (var item in languages)
                {
                    var language = new
                    {
                        id = item.Id,
                        name = item.Name,
                        languageCulture = item.LanguageCulture,
                        uniqueSeoCode = item.UniqueSeoCode,
                        displayOrder = item.DisplayOrder,
                        defaultCurrencyId=item.DefaultCurrencyId
                    };
                    dataList.Add(language);                    
                }
                result.Data = dataList;
            }
            catch
            {
                result.Errors = "E_1003";
            }
            return Json(result);
        }

        /// <summary>
        /// 查询货币表数据API
        /// </summary>           
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllCurrencyes()
        {
            var result = new DataSourceResult();
            try
            {
                IList dataList = new ArrayList();
                var currencys = _currencyService.GetAllCurrencies();
                var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                foreach (var item in currencys)
                {
                    var currency = new
                    {
                        id = item.Id,
                        name = item.Name,                        
                        currencyCode = item.CurrencyCode,
                        rate = item.Rate,
                        customFormatting = item.CustomFormatting,
                        displayOrder = item.DisplayOrder,
                        isDefault= (_currencySettings.PrimaryStoreCurrencyId== item.Id)
                    };
                    dataList.Add(currency);                    
                }
                result.Data = dataList;
            }
            catch
            {
                result.Errors = "E_1003";
            }
            return Json(result);
        }


        /// <summary>
        /// 查询用户属性表数据API
        /// </summary>           
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllUserAttrs()
        {
            var result = new DataSourceResult();
            try
            {
                IList dataList = new ArrayList();
                var userAttrs = _productAttributeService.GetAllProductAttributes();
                foreach (var item in userAttrs)
                {
                    var userAttr = new
                    {
                        id = item.Id,
                        name = item.Name,                        
                    };
                    dataList.Add(userAttr);                    
                }
                result.Data = dataList;
            }
            catch
            {
                result.Errors = "E_1003";
            }
            return Json(result);
        }

        /// <summary>
        /// 查询用户所有的日志记录
        /// </summary>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllLogs(string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();
                var logs = _customerActivityService.GetLesswalletLogs(customer.Id, new int[] { 137, 138 }, null);
                foreach (var item in logs)
                {
                    var log = new
                    {
                        id = item.Id,
                        type = item.ActivityLogTypeId,
                        customerId = item.CustomerId,
                        comment = item.Comment,
                        createdTime = item.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                        readed= !string.IsNullOrEmpty(item.IpAddress)
                    };
                    dataList.Add(log);                    

                    item.IpAddress = _webHelper.GetCurrentIpAddress();
                    _customerActivityService.UpdateLog(item);
                }
                result.Data = dataList;
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 转送电子卷订单给好友
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="friendId">好友ID</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult sentOrderToFriend(string device, int orderId, int friendId)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                try
                {
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null && order.Customer.Id == customer.Id)
                    {
                        Customer friend = _customerService.GetCustomerById(friendId);
                        if (friend != null)
                        {
                            order.Customer = friend;

                            var orderNote = new OrderNote
                            {
                                OrderId = orderId,
                                DisplayToCustomer = false,
                                Note = "",
                                DownloadId = customer.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                            };
                            order.OrderNotes.Add(orderNote);
                            _orderService.UpdateOrder(order);

                            //添加收到电子卷的活动日志
                            _customerActivityService.InsertLog(customer, "Lesswallet.Friend.Coupon.Sent", friend.Id+":"+orderId.ToString());
                            _customerActivityService.InsertLog(friend, "Lesswallet.Friend.Coupon.Received", customer.Id + ":" + orderId.ToString());
                        }
                        else
                        {
                            result.Errors = "E_3001";
                        }
                    }
                }
                catch (Exception exe)
                {
                    result.Errors = "E_3001";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 获取用户最新未读消息API
        /// </summary>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult MsgSync(string device,bool isvendor)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                IList dataList = new ArrayList();
                int[] types;
                if (isvendor)
                {
                    types=new int[] { 139, 140,141,142,143,144,145,146,147,148,149 };
                }
                else
                {
                    types = new int[] { 137, 138,150,151,152,153,154,155,156,157,158,159,160};
                }
                var logs = _customerActivityService.GetLesswalletLogs(customer.Id, types, false);
                foreach (var item in logs)
                {
                    var log = new
                    {
                        id = item.Id,
                        type = item.ActivityLogTypeId,
                        customerId = item.CustomerId,
                        comment = item.Comment,
                        createdTime = item.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                        readed = false
                    };
                    dataList.Add(log);                    

                    item.IpAddress = _webHelper.GetCurrentIpAddress();
                    _customerActivityService.UpdateLog(item);
                }
                result.Data = dataList;                
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 删除日志记录
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult DelLogs(string device, string ids)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    var idArr = ids.Split(',');
                    if (ids != null && ids.Count() > 0)
                    {
                        try
                        {
                            IList dataList = new ArrayList();
                            foreach (var idstr in idArr)
                            {
                                int id = Convert.ToInt32(idstr);
                                var log = _customerActivityService.GetActivityById(id);
                                if (log != null)
                                {
                                    _customerActivityService.DeleteActivity(log);
                                    dataList.Add(id);
                                }
                            }
                            result.Data = dataList;
                        }
                        catch (Exception exe)
                        {
                            result.Errors = "E_3001";
                        }

                    }
                    else
                    {
                        result.Errors = "E_1002";
                    }
                }
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }



        /// <summary>
        /// 获取商户所有已发布的电子卷(商户接口)
        /// </summary>    
        /// <param name="vendorId">商户ID</param>    
        /// <param name="device">设备码</param> 
        /// <param name="productType">1:wicoupon, 2:witicket, 3:wicard</param> 
        /// <param name="productType">当前页索引值</param> 
        /// <param name="pageSize">每页显示记录数</param> 
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllMerchantProducts(int vendorId, string device, int productType, int pageIndex, int pageSize)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var vendor = _vendorService.GetVendorById(vendorId);

                IList dataList = new ArrayList();
                IPagedList<Product> list = _productService.SearchProducts(pageIndex: pageIndex, pageSize: pageSize, vendorId: vendorId, productType: (ProductType?)productType, overridePublished: true, showHidden: true, storeId: vendor.StoreId);
                foreach (var product in list)
                {
                    var couponProduct = new
                    {
                        productId = product.Id,
                        productTypeId = product.ProductTypeId,
                        productTemplateId = product.ProductTemplateId,
                        title = product.Name,
                        shortDesc = product.ShortDescription,
                        fullDesc = product.FullDescription,
                        agreement = product.UserAgreementText,
                        numPrefix = product.Sku,//卡卷编号前缀
                        price = product.Price,
                        currencyCode = _workContext.WorkingCurrency.CurrencyCode,
                        startTime = product.AvailableStartDateTimeUtc.HasValue ? product.AvailableStartDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                        endTime = product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                        specAttr = PrepareProductSpecificationModel(product),
                        userAttr = PrepareProductAttributeModels(product),
                        merchantId = product.VendorId,
                        published = product.Published,
                        deleted = product.Deleted,
                        stockQuantity = product.StockQuantity

                    };
                    dataList.Add(couponProduct);
                }
                result.Data = dataList;
                result.Total = dataList.Count;
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 获取商户所有已发布的电子卷ID列表(商户接口)
        /// </summary>    
        /// <param name="vendorId">商户ID</param>    
        /// <param name="device">设备码</param> 
        /// <param name="productType">1:wicoupon, 2:witicket, 3:wicard</param> 
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GetAllMerchantProductIds(int vendorId, string device, int productType)
        {
            var result = new DataSourceResult();
            //int langID = _mobileAPIService.getLanguageIdByCulture(language);
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var vendor = _vendorService.GetVendorById(vendorId);
                IList dataList = new ArrayList();
                IPagedList<Product> list = _productService.SearchProducts(vendorId: vendorId, productType: (ProductType?)productType, overridePublished: true, showHidden: true, storeId: vendor.StoreId);
                foreach (var product in list)
                {
                    dataList.Add(product.Id);
                }
                result.Data = dataList;
                result.Total = dataList.Count;
            }
            else
            {
                result.Errors = "E_1002";
            }

            return Json(result);
        }

        /// <summary>
        /// 商家根据用户ID查询用户信息，包括用户在此商家的积分(商户接口)
        /// </summary>
        /// <param name="id">用户ID</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult GeMerchantUser(int id, string device)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                var user = _customerService.GetCustomerById(id);
                IList dataList = new ArrayList();
                if (user != null)
                {
                    if (customer.IsVendor())
                    {
                        var vendor = _vendorService.GetVendorById(customer.VendorId);
                        int rewardPointsBalance = _rewardPointService.GetRewardPointsBalance(user.Id, vendor.StoreId);
                        string avatarUrl = _pictureService.GetPictureUrl(user.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), 50, false);
                        var userData = new
                        {
                            id = user.Id,
                            userName = user.Username,
                            email = user.Email,
                            mobile = user.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                            firstName = user.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                            lastName = user.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                            birthday = "",
                            countryId = user.BillingAddress.CountryId,
                            address = user.GetAttribute<string>(SystemCustomerAttributeNames.StreetAddress),
                            avatarUrl = avatarUrl,
                            points = rewardPointsBalance
                        };
                        dataList.Add(userData);
                        result.Data = dataList;
                    }
                    else
                    {

                    }
                }
                else
                {
                    result.Errors = "E_3000";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }

        /// <summary>
        /// 添加或减少用户积分(商户接口)
        /// </summary>
        /// <param name="device"></param>
        /// <param name="uid"></param>
        /// <param name="points"></param>
        /// <param name="storeId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public virtual ActionResult ModifyPoints(string device, int uid, int points, string msg)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                if (customer.IsVendor())//如果是商户才能修改用户的积分
                {
                    var user = _customerService.GetCustomerById(uid);
                    if (user != null)
                    {
                        var vendor = _vendorService.GetVendorById(customer.VendorId);
                        if (points < 0)
                        {
                            int pointsBalance = _rewardPointService.GetRewardPointsBalance(user.Id, vendor.StoreId);
                            if ((pointsBalance + points) >= 0)
                            {
                                if (string.IsNullOrEmpty(msg))
                                {
                                    msg = _localizationService.GetResource("Api.ModifyPoints.Message.MerchantDeduct");
                                }
                                _rewardPointService.AddRewardPointsHistoryEntry(user, points, vendor.StoreId, msg);
                                //添加商户扣减积分的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Point.Redeemed", user.Id + ":" + points.ToString());
                                _customerActivityService.InsertLog(user, "Lesswallet.User.Point.Redeemed", customer.Id + ":" + points.ToString());

                            }
                            else
                            {
                                result.Errors = "E_2050";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(msg))
                            {
                                msg = _localizationService.GetResource("Api.ModifyPoints.Message.MerchantAdd");
                            }
                            _rewardPointService.AddRewardPointsHistoryEntry(user, points, vendor.StoreId, msg);
                            //添加商户送出积分的活动日志
                            _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Point.Added", user.Id + ":" + points.ToString());
                            _customerActivityService.InsertLog(user, "Lesswallet.User.Point.Added", customer.Id + ":" + points.ToString());
                        }
                    }
                    else
                    {
                        result.Errors = "E_2015";
                    }
                }
                else
                {
                    result.Errors = "E_2051";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }

        /// <summary>
        /// 修改预付金额(商户接口)
        /// </summary>
        /// <param name="device"></param>
        /// <param name="orderId"></param>        
        /// <param name="value">要添加或减少的预付款的值</param>        
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public virtual ActionResult ModifyCash(string device, int orderId,String key, string value)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                if (customer.IsVendor())//如果是商户才能修改卡卷金额
                {
                    var order = _orderService.GetOrderById(orderId);
                    if (order != null && order.PaymentStatus == PaymentStatus.Paid && !order.Deleted)
                    {
                        var itme = order.OrderItems.FirstOrDefault();
                        var product = itme.Product;
                        var attrDict = order.DeserializeCustomValues();
                        string bval = null;
                        foreach (var attr in attrDict)
                        {
                            if (attr.Key.Equals("cash"))
                            {
                                if (key.Equals("2"))
                                {
                                    decimal cash = Decimal.Parse((string)attr.Value);
                                    decimal amount = Decimal.Parse(value);
                                    decimal balance = cash + amount;
                                    if (balance < 0)//如果余额不足
                                    {
                                        result.Errors = "E_3003";
                                        return Json(result);
                                    }
                                    else
                                    {
                                        bval = balance.ToString("0.00");

                                    }
                                }
                                else if (key.Equals("3"))
                                {
                                    int totalTime = int.Parse((string)attr.Value);
                                    int time = int.Parse(value);
                                    int balance = totalTime + time;
                                    if (balance < 0)//如果次数不足
                                    {
                                        result.Errors = "E_3003";
                                        return Json(result);
                                    }
                                    else
                                    {
                                        bval = balance.ToString();
                                    }
                                }
                                else if (key.Equals("4"))
                                {
                                    int totalTime = int.Parse((string)attr.Value);
                                    int time = int.Parse(value);
                                    int balance = totalTime + time;
                                    if (balance < 0)//如果购买次数不够减
                                    {
                                        result.Errors = "E_3003";
                                        return Json(result);
                                    }
                                    else
                                    {
                                        bval = balance.ToString();
                                    }
                                }
                                else
                                {
                                    result.Errors = "E_3004";
                                    return Json(result);
                                }

                                break;
                            }
                        }
                        attrDict.Remove(key);
                        attrDict.Add(key, bval);

                        var ds = new DictionarySerializer(attrDict);
                        var xs = new XmlSerializer(typeof(DictionarySerializer));
                        using (var textWriter = new StringWriter())
                        {
                            using (var xmlWriter = XmlWriter.Create(textWriter))
                            {
                                xs.Serialize(xmlWriter, ds);
                            }
                            order.CustomValuesXml = textWriter.ToString();
                        }
                        _orderService.UpdateOrder(order);

                        IList dataList = new ArrayList();
                        var coupon = new
                        {
                            orderId = itme.OrderId,
                            cid = order.CustomOrderNumber,
                            productId = product.Id,
                            userId = customer.Id,
                            orderTotal = order.OrderTotal,
                            paymentStatus = order.PaymentStatusId,
                            startTime = order.PaidDateUtc.HasValue ? order.PaidDateUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : product.CreatedOnUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                            endTime = product.AvailableEndDateTimeUtc.HasValue ? product.AvailableEndDateTimeUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                            deleted = order.Deleted,
                            customValues = order.DeserializeCustomValues()
                            //values = ParseProductAttributeMappingIds(itme.AttributesXml)//用户选择或输入属性值集合
                        };
                        dataList.Add(coupon);
                        result.Data = dataList;
                        if (value.StartsWith("-"))
                        {
                            if (key.Equals("2"))
                            {
                                //添加商户扣减金额的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Cash.Redeemed", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Cash.Redeemed", customer.Id + ":" + order.Id + ":" + value);
                            }
                            else if (key.Equals("3"))
                            {
                                //添加商户扣减服务的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Service.Redeemed", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Service.Redeemed", customer.Id + ":" + order.Id + ":" + value);
                            }
                            else if (key.Equals("4"))
                            {
                                //添加商户扣减购买次数的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Buy.Redeemed", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Buy.Redeemed", customer.Id + ":" + order.Id + ":" + value);
                            }
                        }
                        else
                        {
                            if (key.Equals("2"))
                            {
                                //添加商户送出金额的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Cash.Added", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Cash.Added", customer.Id + ":" + order.Id + ":" + value);
                            }
                            else if (key.Equals("3"))
                            {
                                //添加商户赠送服务的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Service.Added", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Service.Added", customer.Id + ":" + order.Id + ":" + value);
                            }
                            else if (key.Equals("4"))
                            {
                                //添加商户购买次数的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Buy.Added", order.Customer.Id + ":" + order.Id + ":" + value);
                                _customerActivityService.InsertLog(order.Customer, "Lesswallet.User.Buy.Added", customer.Id + ":" + order.Id + ":" + value);
                            }
                        }
                    }
                    else
                    {
                        result.Errors = "E_3002";
                    }
                }
                else
                {
                    result.Errors = "E_2051";
                }
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }


        /// <summary>
        /// 商户发送单个卡卷给用户(商户接口)
        /// </summary>
        /// <param name="device">设备编号</param>
        /// <param name="productId">电子卷ID</param>   
        /// <param name="customerId">用户ID</param>
        /// <returns></returns>
        [System.Web.Mvc.HttpPost]
        public ActionResult SendProductToUser(string device, int productId, int customerId)
        {
            var result = new DataSourceResult();
            string authorization = _httpContext.Request.Headers["Authorization"];
            //不能用_workContext.CurrentCustomer获取此用户，_workContext.CurrentCustomer只能获取临时用户            
            var customer = GetCustomerFromToken(authorization, device);
            if (customer != null)
            {
                if (customer.IsVendor())//如果是商户才能送出产品
                {
                    var user = _customerService.GetCustomerById(customerId);
                    if (user != null)
                    {
                        //清空购物车中的数据          
                        if (user.ShoppingCartItems.Count > 0)
                        {
                            var itemList = user.ShoppingCartItems.ToList();
                            foreach (var item in itemList)
                            {
                                _shoppingCartService.DeleteShoppingCartItem(item);
                            }
                        }
                        //添加当前优惠卷到购物车
                        var product = _productService.GetProductById(productId);
                        var warnings = _shoppingCartService.AddToCart(user,
                        product, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
                        if (warnings.Count > 0)
                        {
                            return Json(result);
                        }

                        var vendor = _vendorService.GetVendorById(customer.VendorId);
                        var processPaymentRequest = new ProcessPaymentRequest();
                        processPaymentRequest.StoreId = _storeContext.CurrentStore.Id;
                        processPaymentRequest.CustomerId = user.Id;                        
                        //保存Coupon的用户权益规则,主要为了以后修改服务次数和充值
                        var attrList = PrepareProductSpecificationModel(product);
                        foreach (var attr in attrList)
                        {
                            if (attr.SpecificationAttributeId >= 1 && attr.SpecificationAttributeId < 4)
                            {
                                processPaymentRequest.CustomValues.Add(attr.SpecificationAttributeId.ToString(), attr.ValueRaw);
                            }
                            else if (attr.SpecificationAttributeId == 4)
                            {
                                processPaymentRequest.CustomValues.Add(attr.SpecificationAttributeId.ToString(), "0");
                            }
                        }
                        var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                        if (placeOrderResult.Success)
                        {
                            if (placeOrderResult.PlacedOrder.PaymentStatus != PaymentStatus.Paid)
                            {                                
                                _orderProcessingService.MarkOrderAsPaid(placeOrderResult.PlacedOrder);
                            }
                            IList dataList = new ArrayList();
                            dataList.Add(placeOrderResult.PlacedOrder.Id);
                            result.Data = dataList;

                            if (product.ProductType == ProductType.WiCoupon)
                            {
                                //添加商户送出电子卷的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Coupon.Sent", user.Id+":"+placeOrderResult.PlacedOrder.Id.ToString());
                                _customerActivityService.InsertLog(user, "Lesswallet.User.Coupon.Received", customer.Id + ":" + placeOrderResult.PlacedOrder.Id.ToString());
                            }
                            if (product.ProductType == ProductType.WiCard)
                            {
                                //添加商户送出会员卡的活动日志
                                _customerActivityService.InsertLog(customer, "Lesswallet.Merchant.Card.Sent", user.Id + ":" + placeOrderResult.PlacedOrder.Id.ToString());
                                _customerActivityService.InsertLog(user, "Lesswallet.User.Card.Received", customer.Id + ":" + placeOrderResult.PlacedOrder.Id.ToString());
                            }
                        }
                        else
                        {

                            result.Errors = "E_3000";
                        }
                    }
                    else
                    {
                        result.Errors = "E_2015";
                    }
                }
                else
                {
                    result.Errors = "E_2051";
                }
                
            }
            else
            {
                result.Errors = "E_1002";
            }
            return Json(result);
        }


        #endregion

    }
}
