using System.Security.Claims;
using System.Web.Mvc;
using Security.Claim;

namespace TestSecurity.Controllers
{
    [HandleError]
    public class BaseController : Controller
    {
        protected UserPrincipalClaims UserClaims
        {
            get
            {
                return new UserPrincipalClaims(User as ClaimsPrincipal);
            }
        }
    }
}