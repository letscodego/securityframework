using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Security.Claim;
using Security.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Web.Mvc;
using Security;

namespace TestSecurity.Controllers
{
    [ClaimsAuthorize(ClaimType = "Admin", ClaimValue = "Groups")]
    public class GroupsAdminController : BaseController
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
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

        public virtual ActionResult Index()
        {
            return View(this.GroupManager.Groups.ToList());
        }

        public virtual async Task<ActionResult> Details(int id)
        {
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationGroup applicationgroup = await this.GroupManager.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if (applicationgroup == null)
            {
                return HttpNotFound();
            }
            var groupRoles = this.GroupManager.GetGroupRoles(applicationgroup.Id);
            var groupClaims = this.GroupManager.GetGroupClaims(applicationgroup.Id);
            string[] roleNames = groupRoles.Select(p => p.Name).ToArray();
            string[] claimNames = groupClaims.Select(p => p.Key + " - " + p.Value).ToArray();
            ViewBag.RolesList = roleNames;
            ViewBag.ClaimsList = claimNames;
            ViewBag.RolesCount = roleNames.Count();
            ViewBag.ClaimsCount = claimNames.Count();
            return View(applicationgroup);
        }

        public virtual ActionResult Create()
        {
            //Get a SelectList of Roles to choose from in the View:
            //ViewBag.RolesList = new SelectList(
            //    this.RoleManager.Roles.ToList(), "Id", "Name");
            ViewBag.ClaimsList = new SelectList(
                this.ClaimManager.Claims.ToList(), "Id", "Value");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Create(ApplicationGroup applicationgroup, params int[] selectedClaimIds)
        {
            if (ModelState.IsValid)
            {
                // Create the new Group:
                var result = await this.GroupManager.CreateGroupAsync(applicationgroup);
                if (result.Succeeded)
                {
                    selectedClaimIds = selectedClaimIds ?? new int[] { };
                    // Add the claims selected:
                    await this.GroupManager.SetGroupClaimsAsync(applicationgroup.Id, selectedClaimIds);
                }
                return RedirectToAction("Index");
            }

            // Otherwise, start over:
            //ViewBag.RolesList = new SelectList(this.RoleManager.Roles.ToList(), "Id", "Name");
            ViewBag.ClaimsList = new SelectList(this.ClaimManager.Claims.ToList(), "Id", "Value");
            return View(applicationgroup);
        }
        public virtual async Task<ActionResult> Edit(int id)
        {
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var applicationgroup = await this.GroupManager.FindByIdAsync(id);
            if (applicationgroup == null)
            {
                return HttpNotFound();
            }
            // Get a list, not a DbSet or queryable:
            var allRoles = await this.RoleManager.Roles.ToListAsync();
            var groupRoles = await this.GroupManager.GetGroupRolesAsync(id);

            //var claims = _db.ApplicationClaims.ToList();
            var allClaims = await this.ClaimManager.Claims.ToListAsync();
            var groupClaims = await this.GroupManager.GetGroupClaimsAsync(id);

            var model = new GroupViewModel()
            {
                Id = applicationgroup.Id,
                Name = applicationgroup.Name,
                Description = applicationgroup.Description
            };
            // load the roles/Roles for selection in the form:
            foreach (var role in allRoles)
            {
                var listItem = new SelectListItem()
                {
                    Text = role.Name,
                    Value = role.Id.ToString(),
                    Selected = groupRoles.Any(g => g.Id == role.Id)
                };
                model.RolesList.Add(listItem);
            }
            foreach (var claim in allClaims)
            {
                var listItem = new SelectListItem()
                {
                    Text = claim.Key + "_" + claim.Value,
                    Value = claim.Id.ToString(),
                    Selected = groupClaims.Any(g => g.Id == claim.Id)
                };
                model.ClaimsList.Add(listItem);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Edit(GroupViewModel model, int[] selectedRoleIds,
            params int[] selectedClaimIds)
        {
            var group = await this.GroupManager.FindByIdAsync(model.Id);
            if (group == null)
            {
                return HttpNotFound();
            }
            if (!ModelState.IsValid) return View(model);
            group.Name = model.Name;
            group.Description = model.Description;
            await this.GroupManager.UpdateGroupAsync(group);

            selectedRoleIds = selectedRoleIds ?? new int[] { };
            await this.GroupManager.SetGroupRolesAsync(group.Id, selectedRoleIds);

            selectedClaimIds = selectedClaimIds ?? new int[] { };
            await this.GroupManager.SetGroupClaimsAsync(group.Id, selectedClaimIds);

            return RedirectToAction("Index");
        }

        public virtual async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationGroup applicationgroup = await this.GroupManager.FindByIdAsync(id);
            if (applicationgroup == null)
            {
                return HttpNotFound();
            }
            return View(applicationgroup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var applicationgroup = await this.GroupManager.FindByIdAsync(id);
            if (applicationgroup == null)
            {
                return HttpNotFound();
            }
            await this.GroupManager.DeleteGroupAsync(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}