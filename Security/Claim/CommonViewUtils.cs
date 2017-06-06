using System.Security.Claims;
using System.Web.Mvc;

namespace Security.Claim
{
    public abstract class CommonViewUtils<TModel> : WebViewPage<TModel>
    {
        protected UserPrincipalClaims UserClaims
        {
            get
            {
                return new UserPrincipalClaims(this.User as ClaimsPrincipal);
            }
        }
    }
    public abstract class CommonViewUtils : CommonViewUtils<dynamic>
    {

    }
}