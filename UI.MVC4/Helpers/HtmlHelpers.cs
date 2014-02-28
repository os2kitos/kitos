using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace UI.MVC4.Helpers
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString TabLink(this HtmlHelper helper, string text, string action, string controller)
        {
            var routeData = helper.ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];

            if (String.Equals(action, currentAction as string, StringComparison.OrdinalIgnoreCase)
                && String.Equals(controller, currentController as string, StringComparison.OrdinalIgnoreCase))
            {
                return helper.ActionLink(text, action, controller, null, new {@class = "btn btn-tab btn-tab-active" });
            }
            return helper.ActionLink(text, action, controller, null, new { @class = "btn btn-tab" });
        }
    }
}