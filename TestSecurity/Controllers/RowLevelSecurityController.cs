using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Security.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Net;
using Security;
using Security.Claim;

namespace TestSecurity.Controllers
{
    [ClaimsAuthorize(ClaimType = "Admin", ClaimValue = "RLS")]
    public class RowLevelSecurityController : BaseController
    {
        public RowLevelSecurityController()
        {
        }

        private RowFilterManager _rowFilterManager;
        public RowFilterManager RowFilterManager
        {
            get
            {
                return _rowFilterManager ?? new RowFilterManager();
            }
            private set
            {
                _rowFilterManager = value;
            }
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

        public ActionResult Index()
        {
            var rows = this.RowFilterManager.RowFilters.ToList();
            var model = new List<PrincipalRowFilterViewModel>();
            try
            {
                foreach (var item in rows)
                {
                    var principalName = item.PrincipalType == "G" ? GroupManager.FindById(item.PrincipalId).Name
                                           : UserManager.FindByIdAsync(item.PrincipalId).Result.Email;
                    var viewModel = new PrincipalRowFilterViewModel
                    {
                        Id = item.Id,
                        PrincipalName = principalName,
                        RowFilterTypeName = item.ApplicationRowFilterType.Name,
                        RowFilterValue = item.RowFilterValue,
                    };
                    model.Add(viewModel);
                }
                return View(model);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public ActionResult Create()
        {
            var model = new ApplicationPrincipalRowFilter()
            {
                PrincipalId = -1
            };
            return View(ModelToDto(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PrincipalRowFilterViewModel rowFilter)
        {
            if (ModelState.IsValid)
            {
                var result = await this.RowFilterManager.CreateRowFiltersAsync(DtoToModel(rowFilter));
                if (result.Succeeded)
                    return RedirectToAction("Index");
            }
            var user = new SelectList(UserManager.Users.ToList(), "Id", "Email");
            var group = new SelectList(GroupManager.Groups.ToList(), "Id", "Name");
            var lst = user.ToList();
            lst.AddRange(group);
            rowFilter.PrincipalList = lst;
            return View(rowFilter);
        }

        public async Task<ActionResult> Edit(int id)
        {
            if (id < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var rowFilter = await this.RowFilterManager.FindByIdAsync(id);
            if (rowFilter == null)
            {
                return HttpNotFound();
            }
            return View(ModelToDto(rowFilter));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PrincipalRowFilterViewModel rowFilter)
        {
            if (ModelState.IsValid)
            {
                var result = await this.RowFilterManager.UpdateRowFilterAsync(DtoToModel(rowFilter));
                if (result.Succeeded)
                    return RedirectToAction("Index");
            }
            var user = new SelectList(UserManager.Users.ToList(), "Id", "Email");
            var group = new SelectList(GroupManager.Groups.ToList(), "Id", "Name");
            var lst = user.ToList();
            lst.AddRange(group);
            rowFilter.PrincipalList = lst;
            return View(rowFilter);
        }

        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationPrincipalRowFilter rowFilter = await this.RowFilterManager.FindByIdAsync(id);
            if (rowFilter == null)
            {
                return HttpNotFound();
            }
            return View(ModelToDto(rowFilter));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var rowFilter = await this.RowFilterManager.FindByIdAsync(id);
            if (rowFilter == null)
            {
                return HttpNotFound();
            }
            await this.RowFilterManager.DeleteRowFilterAsync(id);
            return RedirectToAction("Index");
        }

        private PrincipalRowFilterViewModel ModelToDto(ApplicationPrincipalRowFilter model)
        {
            var principalName = "";
            if (model.PrincipalId != -1)
                principalName = model.PrincipalType == "G" ? GroupManager.FindById(model.PrincipalId).Name
                                                           : UserManager.FindByIdAsync(model.PrincipalId).Result.Email;
            else
                principalName = "";

            var user = new SelectList(UserManager.Users.ToList(), "Id", "Email");
            var group = new SelectList(GroupManager.Groups.ToList(), "Id", "Name");
            var type = new SelectList(RowFilterManager.RowFilterTypes.ToList(), "Id", "Name");

            var lst = user.ToList();
            lst.AddRange(group);
            var viewModel = new PrincipalRowFilterViewModel
            {
                Id = model.Id,
                PrincipalName = principalName,
                PrincipalType = model.PrincipalType,
                SelectedRowFilterTypeId = model.ApplicationRowFilterType != null ? model.ApplicationRowFilterType.Id : 0,
                RowFilterTypeName = model.ApplicationRowFilterType != null ? model.ApplicationRowFilterType.Name : "",
                RowFilterTypeList = type,
                RowFilterValue = model.RowFilterValue,
                PrincipalList = lst,
                SelectedPrincipalId = model.PrincipalId
            };
            return viewModel;
        }

        private ApplicationPrincipalRowFilter DtoToModel(PrincipalRowFilterViewModel viewModel)
        {
            var model = new ApplicationPrincipalRowFilter
            {
                Id = viewModel.Id,
                PrincipalId = viewModel.SelectedPrincipalId,
                PrincipalType = viewModel.PrincipalType,
                RowFilterValue = viewModel.RowFilterValue,
                RowFilterTypeId = viewModel.SelectedRowFilterTypeId
            };
            return model;
        }
    }
}
