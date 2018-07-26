using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Widgets.NivoSlider
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            
            //查询轮播图片数据
            routes.MapRoute("Plugin.Widgets.NivoSlider.GetSliderData",
                 "Plugins/API/GetSliderData",
                 new { controller = "WidgetsNivoSlider", action = "GetSliderData" },
                 new[] { "Nop.Plugin.Widgets.NivoSlider.Controllers" }
            );            
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
