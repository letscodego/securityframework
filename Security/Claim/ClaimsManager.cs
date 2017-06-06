using System.Configuration;
using System.Web;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Linq;
using System.Threading;
using Security.Identity;

namespace Security.Claim
{
    public class ClaimsManager : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
            {
                return base.Authenticate(resourceName, incomingPrincipal);
            }
            else
            {
                var appPrincipal = AddCustomClaimsToPrincipal(incomingPrincipal.Identity.Name);
                CreateSessionSecurityToken(appPrincipal);
                return base.Authenticate(resourceName, appPrincipal);
            }
        }

        public ClaimsPrincipal Authenticate(string userName)
        {
            var appPrincipal = AddCustomClaimsToPrincipal(userName);
            CreateSessionSecurityToken(appPrincipal);
            var userClaims = base.Authenticate(string.Empty, appPrincipal);
            Thread.CurrentPrincipal = userClaims;
            HttpContext.Current.User = userClaims;
            return userClaims;
        }

        //add application specific Claims to user's identity
        private static ClaimsPrincipal AddCustomClaimsToPrincipal(String userName)
        {
            PrincipalContext princiContxt = null;
            UserPrincipal thePrincipal = null;

            //get the Domain context for the Directory Services
            princiContxt = new PrincipalContext(ContextType.Domain);

            //get the user-principal object from the Domain context using the specified username
            thePrincipal = UserPrincipal.FindByIdentity(princiContxt, userName);

            var customClaims = new List<System.Security.Claims.Claim> { new System.Security.Claims.Claim(ClaimTypes.Email, userName),
                new System.Security.Claims.Claim(ClaimTypes.Name, userName) };
            if (userName == "aa@test.com")
            {
                var findItem = customClaims.Find(c => c.Value == "SuperAdmin");
                if (findItem == null)
                    customClaims.Add(new System.Security.Claims.Claim("Group", "SuperAdmin"));
            }
            if (thePrincipal != null)
            {
                if (thePrincipal.Surname != null)
                {
                    customClaims.Add(new System.Security.Claims.Claim(ClaimTypes.WindowsAccountName, thePrincipal.SamAccountName));
                    customClaims.Add(new System.Security.Claims.Claim(ClaimTypes.Surname, thePrincipal.Surname));
                }
                // get all groups the user is a member of
                ////
                //// Todo for a weird error on crm dev server. uncomment the below line if you can solve it!
                //// 
                //customClaims.AddRange(thePrincipal.GetAuthorizationGroups().Select(group =>
                //    new System.Security.Claims.Claim("AD_Group", group.Name)));
                PrincipalSearchResult<Principal> adGroup = thePrincipal.GetAuthorizationGroups();
                var iterGroup = adGroup.GetEnumerator();
                using (iterGroup)
                {
                    while (iterGroup.MoveNext())
                    {
                        try
                        {
                            var p = iterGroup.Current;
                            if (string.IsNullOrEmpty(p.Name))
                                continue;
                            customClaims.Add(new System.Security.Claims.Claim("AD_Group", p.Name));
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                //here you can add any claim type-value pairs, maybe some user settings read from DB.
                var db = new ApplicationDbContext();
                var userManager = new ApplicationUserStore(db);
                var user = userManager.Users.FirstOrDefault(u => u.Email == thePrincipal.UserPrincipalName);

                if (user != null)
                {
                    customClaims.Add(new System.Security.Claims.Claim("UserId", user.Id.ToString()));
                    var claims = user.ApplicationClaims;
                    var groups = user.ApplicationGroups;
                    var rowFilters = user.ApplicationPrincipalRowFilters.Where(x => x.PrincipalType == "U");

                    var groupManager = new ApplicationGroupStore(db);
                    var claimManager = new ApplicationClaimStore(db);
                    var rowFilterManager = new RowFilterStore(db);

                    customClaims.AddRange(groups.Select(group => groupManager.FindById(group.ApplicationGroupId)).Select(g =>
                        new System.Security.Claims.Claim("Group", g.Name)));
                    customClaims.AddRange(claims.Select(claim => claimManager.FindById(claim.ApplicationClaimId)).Select(c =>
                        new System.Security.Claims.Claim(c.Key, c.Value)));
                    customClaims.AddRange(rowFilters.Select(r => rowFilterManager.FindById(r.Id)).Select(c =>
                        new System.Security.Claims.Claim(c.ApplicationRowFilterType.Name, c.RowFilterValue.ToString())));

                    var appgroupManager = new ApplicationGroupManager();
                    var groupList = groups.Select(group => groupManager.FindById(group.ApplicationGroupId));
                    foreach (var item in groupList)
                    {
                        var groupRowFilters = item.ApplicationPrincipalRowFilters.Where(x => x.PrincipalType == "G");
                        customClaims.AddRange(groupRowFilters.Select(r => rowFilterManager.FindById(r.Id)).Select(c =>
                            new System.Security.Claims.Claim(c.ApplicationRowFilterType.Name, c.RowFilterValue.ToString())));

                        foreach (var appclaim in appgroupManager.GetGroupClaims(item.Id))
                        {
                            var claim = new System.Security.Claims.Claim(appclaim.Key, appclaim.Value);
                            var findItem = customClaims.Find(c => c.Value == claim.Value && c.Type == claim.Type);
                            if (findItem == null)
                                customClaims.Add(claim);
                        }
                    }
                }
            }

            //https://msdn.microsoft.com/en-us/library/system.security.claims.authenticationtypes(v=vs.110).aspx
            var theCustomClaimsIdentity = new ClaimsIdentity(customClaims, authenticationType: "Negotiate");//Negotiate | Signing | Sealing

            return new ClaimsPrincipal(theCustomClaimsIdentity);
        }



        //create Session Security token and Write it to session cookie
        private static void CreateSessionSecurityToken(ClaimsPrincipal appPrincipalToStoreInSession)
        {
            var tokenTiomout = ConfigurationManager.AppSettings["TokenTimeout"] ??
                               HttpContext.Current.Session.Timeout.ToString();
            var sessionSecurityToken = new SessionSecurityToken(appPrincipalToStoreInSession, TimeSpan.FromMinutes(Convert.ToInt16(tokenTiomout)))
            {
                IsReferenceMode = true
            };
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
        }
    }
}