using Nop.Core.Domain.Catalog;
using System;
using System.Drawing;
using System.IO;
using ThoughtWorks.QRCode.Codec;

namespace Nop.Services.Common
{
    public interface IQRCodeService
    {
        void SaveQRCodePicture(Product product);
        Image BuildQRCodeImage(string data);
        MemoryStream BuildQRCodeStream(string data);
        string ReadQRCode(string filePath);        
    }
}
