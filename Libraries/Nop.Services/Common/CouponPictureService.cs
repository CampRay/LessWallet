using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Helpers;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Services.Configuration;
using Nop.Core.Domain.Directory;
using Nop.Services.Seo;
using System.IO;
using System.Drawing;
using Nop.Core.Domain.Media;
using Nop.Services.Vendors;
using System.Drawing.Imaging;

/// <summary>
/// 优惠卷图片处理工具类
/// </summary>
namespace Nop.Services.Common
{
    public partial class CouponPictureService : ICouponPictureService
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;


        #region Ctor

        public CouponPictureService(ILocalizationService localizationService,
            ILanguageService languageService,
            IWorkContext workContext,            
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IMeasureService measureService,
            IPictureService pictureService,
            IProductService productService,
            IVendorService vendorService,
            IProductAttributeParser productAttributeParser,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingContext,
            IAddressAttributeFormatter addressAttributeFormatter,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings)
        {
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._workContext = workContext;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._productAttributeParser = productAttributeParser;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._settingContext = settingContext;
            this._currencySettings = currencySettings;
            this._catalogSettings = catalogSettings;            
        }

        #endregion

        #region Utilities
        

        /// <summary>
        /// 创建优惠卷图片
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="couponImage"></param>
        /// <param name="logoImage"></param>
        /// <param name="verdorName"></param>
        /// <param name="title"></param>
        /// <param name="priceStr"></param>
        protected virtual void CreateCoupon(Stream fileStream, Image couponImage, Image logoImage=null, string verdorName=null, string title = null,string priceStr=null)
        {
            Graphics g = Graphics.FromImage(couponImage);
            int width=couponImage.Width;
            int height = couponImage.Height;
            int logoWidth=logoImage.Width;
            int logoHeight = logoImage.Height;
            int newLogoHeight = (int)(height *0.3);
            int newLogoWidth = (int)(logoWidth * (newLogoHeight/ logoHeight));
            int halfWidth = (int)(width * 0.5);
            //绘制logo
            if (logoImage != null) {
                g.DrawImage(logoImage, 15, 15, newLogoWidth, newLogoHeight);
            }
            logoImage.Dispose();

            StringFormat sf = new StringFormat();
            //绘制标题
            int fontSizeTitle = (int)(height * 0.15);                 
            if (!string.IsNullOrEmpty(title))
            {
                Font font = new Font("SimHei", fontSizeTitle, FontStyle.Bold);
                sf.Alignment = StringAlignment.Center;
                g.DrawString(title, font, Brushes.Black, new Rectangle(3, (int)(height / 2 - fontSizeTitle / 2 - 2), width - 6, fontSizeTitle + 2), sf);
                font.Dispose();
            }
            //绘制商家名称
            int fontSizeName = (int)(height * 0.1);
            if (!string.IsNullOrEmpty(verdorName))
            {
                Font font = new Font(FontFamily.GenericSansSerif, fontSizeName, FontStyle.Regular);
                sf.Alignment = StringAlignment.Near;
                g.DrawString(verdorName, font, Brushes.Black, new Rectangle(20, (int)(height-height * 0.2+ fontSizeName*0.3), halfWidth, fontSizeName + 6), sf);
                font.Dispose();
            }

            //绘制价格
            int fontSizePrice = (int)(height * 0.1);
            if (!string.IsNullOrEmpty(priceStr))
            {
                Font font = new Font(FontFamily.GenericSansSerif, fontSizeName, FontStyle.Regular);
                sf.Alignment = StringAlignment.Far;
                g.DrawString(priceStr, font, Brushes.Black, new Rectangle(halfWidth, (int)(height-height * 0.2 + fontSizePrice * 0.3), halfWidth-20, fontSizeName + 6), sf);
                font.Dispose();
            }

            //ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            //ImageCodecInfo ici = null;
            //foreach (ImageCodecInfo codec in codecs)
            //{
            //    if (codec.MimeType.IndexOf("jpeg") > -1)
            //        ici = codec;
            //}
            //EncoderParameters encoderParams = new EncoderParameters();
            //long[] qualityParam = new long[1];
            ////if (quality < 0 || quality > 100)
            ////    quality = 80;

            //qualityParam[0] = 80;//quality;

            //EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityParam);
            //encoderParams.Param[0] = encoderParam;

            //if (ici != null)
            //    couponImage.Save(fileStream, ici, encoderParams);
            //else
            //    couponImage.Save(fileStream, ImageFormat.Jpeg);


            //保存到文件
            couponImage.Save(fileStream,ImageFormat.Jpeg);
            g.Dispose();            
        }

        #endregion

        public void SaveCouponMobilePicture(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var languages = _languageService.GetAllLanguages(true);
            foreach (var language in languages)
            {
                var languageId = language.Id;
                var Name = product.GetLocalized(x => x.Name, languageId, false, false);
                var ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                var FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                var vendorName = product.GetLocalized(x => x.ManufacturerPartNumber, languageId, false, false);

                //加载优惠卷模板图片                               
                string fileName = string.Format("wicoupon_{0}.jpg", product.ProductTemplateId);
                string filePath = Path.Combine(CommonHelper.MapPath("~/content/Images"), fileName);
                Image couponImg = Image.FromFile(filePath);
                var mimeType = MimeTypes.ImageJpeg;

                //加载商家logo  
                Image vendorImg = null;
                var vendor = _vendorService.GetVendorById(product.VendorId);
                var logoPicture = _pictureService.GetPictureById(product.SampleDownloadId);
                if (logoPicture == null)
                {
                    Picture vendorPicture = _pictureService.GetPictureById(vendor.PictureId);
                    MemoryStream vendorMemStream = new MemoryStream(vendorPicture.PictureBinary);
                    vendorImg = Image.FromStream(vendorMemStream);
                    vendorMemStream.Close();
                    vendorPicture = null;
                }
                else
                {
                    MemoryStream vendorMemStream = new MemoryStream(logoPicture.PictureBinary);
                    vendorImg = Image.FromStream(vendorMemStream);
                    vendorMemStream.Close();
                    logoPicture = null;
                }
                if (vendor != null)
                {
                    if (string.IsNullOrEmpty(vendorName))
                    {
                        vendorName = vendor.GetLocalized(x => x.Name, languageId, false, false);
                    }
                }

                string priceStr = _localizationService.GetResource("Products.Price.Free", languageId, false);
                if (product.Price > 0)
                {
                    decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.Price, _workContext.WorkingCurrency);
                    priceStr = _priceFormatter.FormatPrice(finalPrice);
                }

                //string couponFileName = string.Format("coupon_{0}_{1}_{2}.jpg", product.Id, languageId, CommonHelper.GenerateRandomDigitCode(4));
                //string couponFilePath = Path.Combine(CommonHelper.MapPath("~/content/files/ExportImport"), couponFileName);
                //using (var fileStream = new FileStream(couponFilePath, FileMode.Create))
                using (var fileStream = new MemoryStream())
                {
                    CreateCoupon(fileStream, couponImg, vendorImg, vendorName, Name, priceStr);
                    var fileBinary = new byte[fileStream.Length];
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Read(fileBinary, 0, fileBinary.Length);
                    fileStream.Dispose();
                    fileStream.Close();
                    var picture = _pictureService.InsertPicture(fileBinary, mimeType, null);

                    _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(Name));

                    _productService.InsertProductPicture(new ProductPicture
                    {
                        PictureId = picture.Id,
                        ProductId = product.Id,
                        DisplayOrder = language.Id,//以语言ID做为显示图片的顺序
                    });
                    picture = null;
                }
            }
        }

        public void SaveCouponWebPicture(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var languages = _languageService.GetAllLanguages(true);            
            foreach (var language in languages)
            {
                var languageId = language.Id;
                var Name = product.GetLocalized(x => x.Name, languageId, false, false);
                var ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                var FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                var vendorName = product.GetLocalized(x => x.ManufacturerPartNumber, languageId, false, false);
                
                //加载优惠卷模板图片                               
                string fileName = string.Format("wicoupon_{0}.jpg", product.ProductTemplateId);
                string filePath = Path.Combine(CommonHelper.MapPath("~/content/Images"), fileName);
                Image couponImg = Image.FromFile(filePath);
                var mimeType = MimeTypes.ImageJpeg;

                //加载商家logo  
                Image vendorImg = null;
                var vendor = _vendorService.GetVendorById(product.VendorId);
                var logoPicture = _pictureService.GetPictureById(product.SampleDownloadId);
                if (logoPicture == null)
                {
                    Picture vendorPicture = _pictureService.GetPictureById(vendor.PictureId);
                    MemoryStream vendorMemStream = new MemoryStream(vendorPicture.PictureBinary);
                    vendorImg = Image.FromStream(vendorMemStream);
                    vendorMemStream.Close();
                    vendorPicture = null;
                }
                else
                {                    
                    MemoryStream vendorMemStream = new MemoryStream(logoPicture.PictureBinary);
                    vendorImg = Image.FromStream(vendorMemStream);
                    vendorMemStream.Close();
                    logoPicture = null;
                }
                if (vendor != null)
                {
                    if (string.IsNullOrEmpty(vendorName))
                    {
                        vendorName = vendor.GetLocalized(x => x.Name, languageId, false, false);
                    }                    
                }

                string priceStr = _localizationService.GetResource("Products.Price.Free", languageId, false);
                if (product.Price > 0)
                {
                    decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.Price, _workContext.WorkingCurrency);
                    priceStr = _priceFormatter.FormatPrice(finalPrice);
                }

                //string couponFileName = string.Format("coupon_{0}_{1}_{2}.jpg", product.Id, languageId, CommonHelper.GenerateRandomDigitCode(4));
                //string couponFilePath = Path.Combine(CommonHelper.MapPath("~/content/files/ExportImport"), couponFileName);
                //using (var fileStream = new FileStream(couponFilePath, FileMode.Create))
                using (var fileStream = new MemoryStream())
                {
                    CreateCoupon(fileStream, couponImg, vendorImg, vendorName, Name, priceStr);
                    var fileBinary = new byte[fileStream.Length];
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Read(fileBinary, 0, fileBinary.Length);
                    fileStream.Dispose();
                    fileStream.Close();                 
                    var picture=_pictureService.InsertPicture(fileBinary, mimeType, null);

                    _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(Name));

                    _productService.InsertProductPicture(new ProductPicture
                    {
                        PictureId = picture.Id,
                        ProductId = product.Id,
                        DisplayOrder = language.Id,//以语言ID做为显示图片的顺序
                    });
                    picture = null;
                }                
            }
        }
    }
}
