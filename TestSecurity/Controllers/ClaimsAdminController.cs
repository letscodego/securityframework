using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Security.Claim;
using Security.Identity;

namespace TestSecurity.Controllers
{
    [ClaimsAuthorize(ClaimType = "Admin", ClaimValue = "Claims")]
    public class ClaimsAdminController : BaseController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
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
            return View(this.ClaimManager.Claims.ToList());
        }


        public virtual ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Create(ApplicationClaim applicationClaim)
        {
            if (ModelState.IsValid)
            {
                await this.ClaimManager.CreateGroupAsync(applicationClaim);
                return RedirectToAction("Index");
            }

            return View(applicationClaim);
        }
        public virtual async Task<ActionResult> Edit(int id)
        {
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationClaim applicationClaim = await this.ClaimManager.FindByIdAsync(id);
            if (applicationClaim == null)
            {
                return HttpNotFound();
            }
            var model = new ClaimViewModel()
            {
                Id = applicationClaim.Id,
                ExpireDate = applicationClaim.ExpireDate,
                Key = applicationClaim.Key,
                Value = applicationClaim.Value
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Edit([Bind(Include = "Id,Key,Value,ExpireDate")] ClaimViewModel model)
        {
            var applicationClaim = await this.ClaimManager.FindByIdAsync(model.Id);
            if (applicationClaim == null)
            {
                return HttpNotFound();
            }
            if (ModelState.IsValid)
            {
                applicationClaim.ExpireDate = model.ExpireDate;
                applicationClaim.Key = model.Key;
                applicationClaim.Value = model.Value;
                await this.ClaimManager.UpdateClaimAsync(applicationClaim);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public virtual async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var applicationClaim = await this.ClaimManager.FindByIdAsync(id);
            if (applicationClaim == null)
            {
                return HttpNotFound();
            }
            return View(applicationClaim);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var applicationClaim = await this.ClaimManager.FindByIdAsync(id);
            await this.ClaimManager.DeleteClaimAsync(id);
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