using System;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Security.Identity;
using System.Linq;

namespace Security
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }

    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            Microsoft.Owin.IOwinContext context)
        {
            var manager = new ApplicationUserManager(new ApplicationUserStore(context.Get<ApplicationDbContext>()));
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.RegisterTwoFactorProvider("PhoneCode",
                new PhoneNumberTokenProvider<ApplicationUser, int>
                {
                    MessageFormat = "Your security code is: {0}"
                });

            manager.RegisterTwoFactorProvider("EmailCode",
                new EmailTokenProvider<ApplicationUser, int>
                {
                    Subject = "SecurityCode",
                    BodyFormat = "Your security code is {0}"
                });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser, int>(
                        dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        public override async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            if (user.IsActiveDirectoryUser)
            {
                return await Task.FromResult(AuthenticateAd(user.UserName, password));
            }
            return await base.CheckPasswordAsync(user, password);
        }
        public bool AuthenticateAd(string username, string password)
        {
            using (var context = new PrincipalContext(ContextType.Domain, "aa"))
            {
                return context.ValidateCredentials(username, password, ContextOptions.Negotiate);
            }
        }
    }

    public class ApplicationRoleManager : RoleManager<ApplicationRole, int>
    {
        public ApplicationRoleManager(IRoleStore<ApplicationRole, int> roleStore)
            : base(roleStore)
        {
        }
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(
                new ApplicationRoleStore(context.Get<ApplicationDbContext>()));
        }
    }

    public class ApplicationDbInitializer : IDatabaseInitializer<ApplicationDbContext>
    {
        private void InitializeIdentityForEf(ApplicationDbContext db)
        {
            const string name = "aa@test.com";
            const string password = "Admin@123456.";

            PasswordHasher hasher = new PasswordHasher();
            ApplicationUser adminUser = new ApplicationUser {
                UserName = name,
                Email = name,
                PasswordHash = hasher.HashPassword(password),
                LockoutEnabled = false,
                EmailConfirmed = true,
                IsActiveDirectoryUser = true
            };

            var adminUserResult = db.Users.FirstOrDefault<ApplicationUser>(x => x.UserName == adminUser.UserName);
            if (adminUserResult == null)
            {
                adminUserResult = db.Users.Add(adminUser);
                db.SaveChanges();
            }
            
            //Because of lazy loading of OwinContext, the method(FindByName) hange in diversity conditions then it's commented by me
            
            //db.Configuration.LazyLoadingEnabled = true;
            //var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            //var user = userManager.FindByName(name);
            //if (user == null)
            //{
            //    user = new ApplicationUser
            //    {
            //        UserName = name,
            //        PasswordHash = hasher.HashPassword(password),
            //        Email = name,
            //        EmailConfirmed = true,
            //        IsActiveDirectoryUser = true
            //    };
            //    var result = userManager.Create(user, password);
            //    result = userManager.SetLockoutEnabled(user.Id, false);
            //}

            var groupManager = new ApplicationGroupManager();
            var newGroup = new ApplicationGroup("SuperAdmin", "Full Access to All");
            if (!groupManager.Groups.Any(x => x.Name == newGroup.Name))
            {
                groupManager.CreateGroup(newGroup);
                groupManager.SetUserGroups(adminUserResult.Id, new int[] { newGroup.Id });
            }
            else
            {
                var group = groupManager.Groups.FirstOrDefault(x => x.Name == newGroup.Name);
                if (!group.ApplicationUsers.Any(x => x.ApplicationUserId == adminUserResult.Id))
                    groupManager.SetUserGroups(adminUserResult.Id, new int[] { group.Id });
            }
        }

        public void InitializeDatabase(ApplicationDbContext context)
        {
            //uncomment for first time that you want to lunch app. For later it's better to be comment. Be careful
            InitializeIdentityForEf(context);
        }
    }

    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(
            ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        { }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        //{
        //    return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        //}
        public override async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            var identity = await base.CreateUserIdentityAsync(user);
            identity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Email, user.Email));
            //if (user.Email == "aa@test.com")
            //{
            //    if (!identity.HasClaim(c => c.Value == "Admin"))
            //        identity.AddClaim(new System.Security.Claims.Claim("Role", "Admin"));
            //}
            return identity;
        }

        public static ApplicationSignInManager Create(
         IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
