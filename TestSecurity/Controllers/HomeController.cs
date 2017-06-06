using Security.Claim;
using System.Web.Mvc;

namespace TestSecurity.Controllers
{
    [ClaimsAuthorize]
    public class HomeController : BaseController
    {
        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult About()
        {
            //ViewBag.AppName = Properties.Resources.AppLongName;
            return View();
        }

        public virtual ActionResult Contact()
        {
            ViewBag.Message = "You may feedback us using the follwing info. preferred method is email.";
            return View();
        }
        public virtual ActionResult Overview()
        {
            return View();
        }
    }
}