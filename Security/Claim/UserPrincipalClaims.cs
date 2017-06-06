using System.Security.Claims;

namespace Security.Claim
{
    public class UserPrincipalClaims:ClaimsPrincipal
    {
        public UserPrincipalClaims(ClaimsPrincipal principal)
            : base(principal)
        {
        }

        public string Name
        {
            get { return this.FindFirst(ClaimTypes.Name).Value; }
        }
        public string Surname
        {
            get
            {
                return this.FindFirst(ClaimTypes.Surname)!=null ? this.FindFirst(ClaimTypes.Surname).Value : "";
            }
        }

        public string Email
        {
            get { return this.FindFirst(ClaimTypes.Email).Value; }
        }

        public string WindowsAccountName
        {
            get { return this.FindFirst(ClaimTypes.WindowsAccountName).Value; }
        }

        public string UserId => this.FindFirst("UserId").Value;
    }
}