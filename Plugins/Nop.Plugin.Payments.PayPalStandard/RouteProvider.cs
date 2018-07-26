using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PayPalStandard
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //PDT
            routes.MapRoute("Plugin.Payments.PayPalStandard.PDTHandler",
                 "Plugins/PaymentPayPalStandard/PDTHandler",
                 new { controller = "PaymentPayPalStandard", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            );
            //IPN
            routes.MapRoute("Plugin.Payments.PayPalStandard.IPNHandler",
                 "Plugins/PaymentPayPalStandard/IPNHandler",
                 new { controller = "PaymentPayPalStandard", action = "IPNHandler" },
                 new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            );
            //Cancel
            routes.MapRoute("Plugin.Payments.PayPalStandard.CancelOrder",
                 "Plugins/PaymentPayPalStandard/CancelOrder",
                 new { controller = "PaymentPayPalStandard", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            );
            //clientToken
            routes.MapRoute("Plugin.Payments.PayPalStandard.GetClientToken",
                 "Plugins/PaymentPayPalStandard/GetClientToken",
                 new { controller = "PaymentPayPalStandard", action = "GetClientToken" },
                 new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
            );

            //Create Transaction
            routes.MapRoute("Plugin.Payments.PayPalStandard.CreateTransaction",
                 "Plugins/PaymentPayPalStandard/CreateTransaction",
                 new { controller = "PaymentPayPalStandard", action = "CreateTransaction" },
                 new[] { "Nop.Plugin.Payments.PayPalStandard.Controllers" }
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
