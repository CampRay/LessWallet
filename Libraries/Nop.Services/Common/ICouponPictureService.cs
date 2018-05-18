using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Common
{
    public partial interface ICouponPictureService
    {
        void SaveCouponWebPicture(Product product);

        void SaveCouponMobilePicture(Product product);
    }
}
