using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.MobileAPI
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //搜索用户
            routes.MapRoute("Plugin.Misc.UserAPI.GetUserById",
                 "Plugins/API/GeUserById",
                 new { controller = "UserAPI", action = "GeUserById" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //搜索用户
            routes.MapRoute("Plugin.Misc.UserAPI.SearchUsers",
                 "Plugins/API/SearchUsers",
                 new { controller = "UserAPI", action = "SearchUsers" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //搜索好友
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllFriends",
                 "Plugins/API/GetAllFriends",
                 new { controller = "UserAPI", action = "GetAllFriends" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );

            //添加好友
            routes.MapRoute("Plugin.Misc.UserAPI.AddFriend",
                 "Plugins/API/AddFriend",
                 new { controller = "UserAPI", action = "AddFriend" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //删除好友
            routes.MapRoute("Plugin.Misc.UserAPI.DelFriend",
                 "Plugins/API/DelFriend",
                 new { controller = "UserAPI", action = "DelFriend" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );

            //用户注册
            routes.MapRoute("Plugin.Misc.UserAPI.Register",
                 "Plugins/API/Register",
                 new { controller = "UserAPI", action = "Register" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //用户login
            routes.MapRoute("Plugin.Misc.UserAPI.Login",
                 "Plugins/API/Login",
                 new { controller = "UserAPI", action = "Login" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //编辑用户信息
            routes.MapRoute("Plugin.Misc.UserAPI.EditUser",
                 "Plugins/API/EditUser",
                 new { controller = "UserAPI", action = "EditUser" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );


            //根据商家ID查询商家信息
            routes.MapRoute("Plugin.Misc.UserAPI.GetMerchant",
                 "Plugins/API/GetMerchant",
                 new { controller = "UserAPI", action = "GetMerchant" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //根据卡卷商品ID查询获取单个卡卷商品信息API
            routes.MapRoute("Plugin.Misc.UserAPI.GetProduct",
                 "Plugins/API/GetProduct",
                 new { controller = "UserAPI", action = "GetProduct" },                 
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //确认购买或获取单个电子卷API
            routes.MapRoute("Plugin.Misc.UserAPI.CreateOrder",
                 "Plugins/API/CreateOrder",
                 new { controller = "UserAPI", action = "CreateOrder" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //查询用户已有的单个电子卷API
            routes.MapRoute("Plugin.Misc.UserAPI.GetOrder",
                 "Plugins/API/GetCoupon",
                 new { controller = "UserAPI", action = "GetOrder" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );

            //初始化运行时获取用户已有的所有电子卷
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllOrders",
                 "Plugins/API/GetAllCoupons",
                 new { controller = "UserAPI", action = "GetAllOrders" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );

            //删除电子卷
            routes.MapRoute("Plugin.Misc.UserAPI.DeleteCoupons",
                 "Plugins/API/DeleteCoupons",
                 new { controller = "UserAPI", action = "DelOrders" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //转送电子卷
            routes.MapRoute("Plugin.Misc.UserAPI.SentCoupon",
                 "Plugins/API/SentCoupon",
                 new { controller = "UserAPI", action = "sentOrderToFriend" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );


            //消息同步
            routes.MapRoute("Plugin.Misc.UserAPI.MsgSync",
                 "Plugins/API/MsgSync",
                 new { controller = "UserAPI", action = "MsgSync" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
           

            //查询国家表数据
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllCountries",
                 "Plugins/API/GetAllCountries",
                 new { controller = "UserAPI", action = "GetAllCountries" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //查询语言表数据
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllLanguages",
                 "Plugins/API/GetAllLanguages",
                 new { controller = "UserAPI", action = "GetAllLanguages" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //查询货币表数据
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllCurrencyes",
                 "Plugins/API/GetAllCurrencyes",
                 new { controller = "UserAPI", action = "GetAllCurrencyes" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
            );
            //查询用户属性表数据
            routes.MapRoute("Plugin.Misc.UserAPI.GetAllUserAttrs",
                 "Plugins/API/GetAllUserAttrs",
                 new { controller = "UserAPI", action = "GetAllUserAttrs" },
                 new[] { "Nop.Plugin.Misc.MobileAPI.Controllers" }
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
