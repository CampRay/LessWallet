using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

/// <summary>
/// QR Code二维码处理工具类
/// </summary>
namespace Nop.Services.Common
{
    public class QRCodeService : IQRCodeService
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

        public QRCodeService(ILocalizationService localizationService,
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

        /// <summary>
        /// 为卡卷新添加二维码图片
        /// </summary>
        /// <param name="product"></param>
        /// <param name="productId"></param>
        public void SaveQRCodePicture(Product product)
        {
            var url = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("product:" + product.ProductTypeId + ":" + product.Id));
            MemoryStream ms = BuildQRCodeStream(url);
            var fileBinary = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(fileBinary, 0, fileBinary.Length);
            ms.Dispose();
            ms.Close();
            //如果之前有生成过二维码图片，则先删除
            if (product.DownloadId > 0)
            {
                _pictureService.UpdatePicture(product.DownloadId, fileBinary, MimeTypes.ImageJpeg, null);
            }
            else
            {
                var picture = _pictureService.InsertPicture(fileBinary, MimeTypes.ImageJpeg, null);
                product.IsDownload = true;
                product.DownloadId = picture.Id;
                _productService.UpdateProduct(product);
                picture = null;
            }
                                  
        }
        

        /// <summary>
        /// 生成二维码图片
        /// </summary>
        /// <param name="data">内容数据</param>
        /// <returns>Image</returns>
        public Image BuildQRCodeImage(string data)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            options.DisableECI = true;
            //设置内容编码
            options.CharacterSet = "UTF-8";
            //设置QR码的纠错通级别L(7%),M(15%),Q(25%),H(30%)
            options.ErrorCorrection = ErrorCorrectionLevel.M;
            //设置QR码的版本，版本越大存储的数据越多
            options.QrVersion = 4;
            //设置二维码的宽度和高度
            options.Width = 300;
            options.Height = 300;
            //设置二维码的边距,单位不是固定像素
            options.Margin = 1;
            writer.Options = options;
            Image image = writer.Write(data);
            return image;
        }

        /// <summary>
        /// 生成二维码图片数据流
        /// </summary>
        /// <param name="data">内容数据</param>
        /// <returns>MemoryStream</returns>
        public MemoryStream BuildQRCodeStream(string data)
        {            
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            options.DisableECI = true;
            //设置内容编码
            options.CharacterSet = "UTF-8";
            //设置QR码的纠错通级别L(7%),M(15%),Q(25%),H(30%)
            options.ErrorCorrection = ErrorCorrectionLevel.M;
            //设置QR码的版本，版本越大存储的数据越多
            options.QrVersion = 4;            
            //设置二维码的宽度和高度
            options.Width = 300;
            options.Height = 300;
            //设置二维码的边距,单位不是固定像素
            options.Margin = 1;
            writer.Options = options;

            Image image = writer.Write(data);

            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms;
        }

        /// <summary>
        /// 二维码解码
        /// </summary>
        /// <param name="filePath">图片路径</param>
        /// <returns></returns>
        public string ReadQRCode(string filePath)
        {            
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            Bitmap map = new Bitmap(filePath);
            Result result = reader.Decode(map);
            return result == null ? "" : result.Text;

        }
        
        
    }
}
