using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.MobileAPI
{
    public class MobileAPISettings : ISettings
    {
        /// <summary>
        /// 移动端访问Token的加密密钥
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// Product picture size
        /// </summary>
        public int ProductPictureSize { get; set; }
              
        /// <summary>
        /// QR Code编码模式
        /// </summary>
        public int QRCodeEncodeMode { get; set; }

        /// <summary>
        /// QR Code像素宽度
        /// </summary>
        public int QRCodeScale { get; set; }

        /// <summary>
        /// QR Code版本
        /// </summary>
        public int QRCodeVersion { get; set; }

        /// <summary>
        /// QR Code纠错级别
        /// </summary>
        public int QRCodeErrorCorrect { get; set; }
    }
}
