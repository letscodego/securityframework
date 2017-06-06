using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Security.Claim
{
    public class ClaimsAuthorizeAttribute : AuthorizeAttribute
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        protected override bool AuthorizeCore(HttpContextBase context)
        {
            var claimsIdentity = new ClaimsIdentity();
            var isAuthorized = base.AuthorizeCore(context);

            if (!context.Request.IsAuthenticated)
            {
                if (context.Request.IsAjaxRequest())
                    throw new AuthenticationException();
            }
            if (!context.Request.IsAuthenticated || !isAuthorized)
            {
                return false;
            }

            //check if the SessionSecurityToken is available in cookie, this will not be available during the first request to the application by a user.
            if (FederatedAuthentication.SessionAuthenticationModule != null && FederatedAuthentication.SessionAuthenticationModule.ContainsSessionTokenCookie(context.Request.Cookies))
            {
                SessionSecurityToken token;
                FederatedAuthentication.SessionAuthenticationModule.TryReadSessionTokenFromCookie(out token);

                claimsIdentity = token.ClaimsPrincipal.Identity as ClaimsIdentity;
            }
            else
            {
                //else get the principal with Custom claims identity using CustomClaimsTransformer, which also sets it in cookie
                var currentPrincipal = context.User as ClaimsPrincipal; // ClaimsPrincipal.Current;
                var appClaimsMgr = new ClaimsManager();
                var tranformedClaimsPrincipal = appClaimsMgr.Authenticate(string.Empty, currentPrincipal);
                Thread.CurrentPrincipal = tranformedClaimsPrincipal;
                HttpContext.Current.User = tranformedClaimsPrincipal;
                claimsIdentity = tranformedClaimsPrincipal.Identities.First() as ClaimsIdentity;
            }
            if(ClaimType== null && ClaimValue == null)
                return base.AuthorizeCore(context);

            isAuthorized = CheckClaimValidity(claimsIdentity, ClaimType, ClaimValue);
            return isAuthorized;
        }


        //checks Claim type/value in the given Claims Identity
        private static Boolean CheckClaimValidity(ClaimsIdentity pClaimsIdentity, string pClaimType, string pClaimValue)
        {
            var blnClaimsValiditiy = false;
            //now check the passed in Claimtype has the passed in Claimvalue
            if (pClaimType == null || pClaimValue == null) return false;
            //ignore the golden user from authorize 
            if (pClaimsIdentity.HasClaim("Group", "SuperAdmin")) return true;
            if (pClaimsIdentity.HasClaim(x => String.Equals(x.Type, pClaimType, StringComparison.CurrentCultureIgnoreCase) && String.Equals(x.Value, pClaimValue, StringComparison.CurrentCultureIgnoreCase)))
            {
                blnClaimsValiditiy = true;
            }
            return blnClaimsValiditiy;
        }

        //Called when access is denied
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("You haven't access to this action!");
            base.HandleUnauthorizedRequest(filterContext);
        }
    }





    //public override Task OnAuthorization(AuthorizationContext context) //, System.Threading.CancellationToken cancellationToken)
    //Core authentication, called before each action that is decoracted with authorize or appClaimsAuthorize attribute
}