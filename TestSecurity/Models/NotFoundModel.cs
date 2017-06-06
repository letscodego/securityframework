using System;
using System.Web.Mvc;

namespace TestSecurity.Models
{
    public class NotFoundModel : HandleErrorInfo
    {
        public NotFoundModel(Exception exception, string controllerName, string actionName)
            : base(exception, controllerName, actionName)
        {
        }
        public string RequestedUrl { get; set; }
        public string ReferrerUrl { get; set; }
    }
}