using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Security.Identity
{
    public class RowFilterManager
    {
        private readonly RowFilterStore _rowFilterStore;
        private readonly ApplicationDbContext _db;
        private readonly ApplicationUserManager _userManager;

        public RowFilterManager()
        {
            _db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            _rowFilterStore = new RowFilterStore(_db);
        }

        public RowFilterManager(ApplicationDbContext dbContext, ApplicationUserManager userManager)
        {
            _db = dbContext;
            _userManager = userManager;
            _rowFilterStore = new RowFilterStore(_db);
        }

        public IQueryable<ApplicationPrincipalRowFilter> RowFilters
        {
            get
            {
                return _rowFilterStore.RowFilters;
            }
        }

        public IQueryable<ApplicationRowFilterType> RowFilterTypes
        {
            get
            {
                return _rowFilterStore.RowFilterTypes;
            }
        }

        public async Task<IdentityResult> CreateRowFiltersAsync(ApplicationPrincipalRowFilter rowFilter)
        {
            await _rowFilterStore.CreateAsync(rowFilter);
            return IdentityResult.Success;
        }

        public IdentityResult CreateRowFilter(ApplicationPrincipalRowFilter rowFilter)
        {
            _rowFilterStore.Create(rowFilter);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteRowFilterAsync(int rowFilterId)
        {
            var rowFilter = await this.FindByIdAsync(rowFilterId);
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilterId");
            }
            _db.ApplicationPrincipalRowFilter.Remove(rowFilter);
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public IdentityResult DeleteRowFilter(int rowFilterId)
        {
            var rowFilter = this.FindById(rowFilterId);
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilterId");
            }
            // Remove the group itself:
            _db.ApplicationPrincipalRowFilter.Remove(rowFilter);
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateRowFilterAsync(ApplicationPrincipalRowFilter rowFilter)
        {
            await _rowFilterStore.UpdateAsync(rowFilter);
            return IdentityResult.Success;
        }

        public IdentityResult UpdateRowFilter(ApplicationPrincipalRowFilter rowFilter)
        {
            _rowFilterStore.Update(rowFilter);
            return IdentityResult.Success;
        }

        public async Task<ApplicationPrincipalRowFilter> FindByIdAsync(int id)
        {
            return await _rowFilterStore.FindByIdAsync(id);
        }

        public ApplicationPrincipalRowFilter FindById(int id)
        {
            return _rowFilterStore.FindById(id);
        }
    }
}
