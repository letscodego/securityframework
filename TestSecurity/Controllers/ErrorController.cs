using System;
using System.Web;
using System.Web.Mvc;
using TestSecurity.Models;

namespace TestSecurity.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult NotFound(string url)
        {
            var originalUri = url ?? Request.QueryString["aspxerrorpath"] ?? Request.Url.OriginalString;
            var controllerName = (string)RouteData.Values["controller"];
            var actionName = (string)RouteData.Values["action"];
            var model = new NotFoundModel(new HttpException(404, "Failed to find page"), controllerName, actionName)
            {
                RequestedUrl = originalUri,
                ReferrerUrl = Request.UrlReferrer == null ? "" : Request.UrlReferrer.OriginalString
            };
            Response.StatusCode = 404;
            return View("NotFound", model);
        }

        protected override void HandleUnknownAction(string actionName)
        {
            var name = GetViewName(ControllerContext, string.Format("~/Views/Error/{0}.cshtml", actionName),
                "~/Views/Error/Error.cshtml",
                "~/Views/Error/UnknownError.cshtml",
                "~/Views/Shared/Error.cshtml");
            var controllerName = (string)RouteData.Values["controller"];
            var ex = Server.GetLastError() ?? new Exception("Unhandled Error");
            var model = new HandleErrorInfo(ex, controllerName, actionName);
            var result = new ViewResult
            {
                ViewName = name,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
            };
            Response.StatusCode = 501;
            result.ExecuteResult(ControllerContext);
        }

        protected string GetViewName(ControllerContext context, params string[] names)
        {
            foreach (var name in names)
            {
                var result = ViewEngines.Engines.FindView(ControllerContext, name, null);
                if (result.View != null)
                    return name;
            }
            return null;
        }
    }
}