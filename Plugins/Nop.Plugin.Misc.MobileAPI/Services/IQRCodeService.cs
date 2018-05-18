using System;
using System.Drawing;
using System.IO;
using ThoughtWorks.QRCode.Codec;

namespace Nop.Plugin.Misc.MobileAPI.Services
{
    public interface IQRCodeService
    {
        Image BuildQRCodeImage(string data);
        MemoryStream BuildQRCodeStream(string data);
        string ReadQRCode(string filePath);

        Image CreateQRCode(string Content, QRCodeEncoder.ENCODE_MODE QRCodeEncodeMode, QRCodeEncoder.ERROR_CORRECTION QRCodeErrorCorrect, int QRCodeVersion, int QRCodeScale, int size, int border);
    }
}
