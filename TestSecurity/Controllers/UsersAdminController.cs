using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Security.Claim;
using Security.Identity;
using Microsoft.AspNet.Identity.Owin;
using Security;

namespace TestSecurity.Controllers
{
    [ClaimsAuthorize(ClaimType = "Admin", ClaimValue = "Users")]
    public class UsersAdminController : BaseController
    {
        public UsersAdminController()
        {
        }
        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationGroupManager _groupManager;
        public ApplicationGroupManager GroupManager
        {
            get
            {
                return _groupManager ?? new ApplicationGroupManager();
            }
            private set
            {
                _groupManager = value;
            }
        }

        private ApplicationClaimManager _claimManager;
        public ApplicationClaimManager ClaimManager
        {
            get
            {
                return _claimManager ?? new ApplicationClaimManager();
            }
            private set
            {
                _claimManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }

            private set
            {
                _roleManager = value;
            }
        }

        public virtual async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.OrderBy(x => x.UserName).ToListAsync());
        }

        public virtual async Task<ActionResult> Details(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id);
            var userGroups = await this.GroupManager.GetUserGroupsAsync(id);
            var userClaims = await this.ClaimManager.GetUserClaimsAsync(id);
            ViewBag.GroupNames = userGroups.Select(u => u.Name).ToList();
            ViewBag.ClaimNames = userClaims.Select(u => u.Key + " - " + u.Value).ToList();
            return View(user);
        }
        public virtual ActionResult Create()
        {
            ViewBag.GroupsList = new SelectList(this.GroupManager.Groups, "Id", "Name");
            ViewBag.ClaimsList = new SelectList(this.ClaimManager.Claims, "Id", "Value");
            return View();
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create(RegisterViewModel userViewModel, int[] selectedGroups, params int[] selectedClaims)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = userViewModel.Email,
                    Email = userViewModel.Email,
                    IsActiveDirectoryUser = userViewModel.IsActiveDirectoryUser
                };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);
                //Add User to the selected Groups 
                if (adminresult.Succeeded)
                {
                    if (selectedGroups != null)
                    {
                        selectedGroups = selectedGroups ?? new int[] { };
                        await this.GroupManager
                            .SetUserGroupsAsync(user.Id, selectedGroups);
                    }
                    if (selectedClaims == null) return RedirectToAction("Index");
                    selectedClaims = selectedClaims ?? new int[] { };
                    await this.ClaimManager
                        .SetUserClaimsAsync(user.Id, selectedClaims);
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in adminresult.Errors)
                    {
                        ModelState.AddModelError("key", item);
                    }

                }
            }

            ViewBag.GroupsList = new SelectList(this.GroupManager.Groups, "Id", "Name");
            ViewBag.ClaimsList = new SelectList(this.ClaimManager.Claims, "Id", "Value");
            return View();
        }
        public virtual async Task<ActionResult> Edit(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var allGroups = this.GroupManager.Groups;
            var userGroups = await this.GroupManager.GetUserGroupsAsync(id);

            var allClaims = this.ClaimManager.Claims;
            var userClaims = await this.ClaimManager.GetUserClaimsAsync(id);

            var model = new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                IsActiveDirectoryUser = user.IsActiveDirectoryUser
            };

            foreach (var group in allGroups)
            {
                var listItem = new SelectListItem()
                {
                    Text = group.Name,
                    Value = group.Id.ToString(),
                    Selected = userGroups.Any(g => g.Id == group.Id)
                };
                model.GroupsList.Add(listItem);
            }


            foreach (var claim in allClaims)
            {
                var listItem = new SelectListItem()
                {
                    Text = claim.Key + " - " + claim.Value,
                    Value = claim.Id.ToString(),
                    Selected = userClaims.Any(g => g.Id == claim.Id)
                };
                model.ClaimsList.Add(listItem);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Edit([Bind(Include = "Email,Id,IsActiveDirectoryUser")] EditUserViewModel editUser, int[] selectedGroups,
            int[] selectedClaims, params int[] selectedAccesses)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update the User:
                user.UserName = editUser.Email;
                user.Email = editUser.Email;
                user.IsActiveDirectoryUser = editUser.IsActiveDirectoryUser;
                await this.UserManager.UpdateAsync(user);
                // Update the Groups:
                selectedGroups = selectedGroups ?? new int[] { };
                await this.GroupManager.SetUserGroupsAsync(user.Id, selectedGroups);
                // Update the Claims:
                selectedClaims = selectedClaims ?? new int[] { };
                await this.ClaimManager.SetUserClaimsAsync(user.Id, selectedClaims);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        public virtual async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid) return View();
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            await this.GroupManager.ClearUserGroupsAsync(id);
            await this.ClaimManager.ClearUserClaimsAsync(id);
            // Then Delete the User:
            var result = await UserManager.DeleteAsync(user);
            if (result.Succeeded) return RedirectToAction("Index");
            ModelState.AddModelError("", result.Errors.First());
            return View();
        }
    }
}
