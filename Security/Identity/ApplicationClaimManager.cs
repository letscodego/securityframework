using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Security.Identity
{
    public class ApplicationClaimManager:IDisposable
    {
        private readonly ApplicationClaimStore _claimStore;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _db;

        public ApplicationClaimManager()
        {
            _db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            _claimStore = new ApplicationClaimStore(_db);
        }
        public ApplicationClaimManager(ApplicationDbContext dbContext)
        {
            _db = dbContext;
            _claimStore = new ApplicationClaimStore(_db);
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        }

        public IQueryable<ApplicationClaim> Claims
        {
            get
            {
                return _claimStore.Claims;
            }
        }

        public async Task<IdentityResult> CreateGroupAsync(ApplicationClaim claim)
        {
            await _claimStore.CreateAsync(claim);
            return IdentityResult.Success;
        }

        public IdentityResult CreateGroup(ApplicationClaim claim)
        {
            _claimStore.Create(claim);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetUserClaimsAsync(int userId, params int[] claimIds)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);
            currentUser.ApplicationClaims.Clear();
            foreach (var claimId in claimIds)
            {
                currentUser.ApplicationClaims.Add(new ApplicationUserClaim
                {
                    ApplicationUserId = userId,
                    ApplicationClaimId = claimId
                });
            }
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public IdentityResult SetUserClaims(int userId, params int[] claimIds)
        {
            var currentUser = _userManager.FindById(userId);
            currentUser.ApplicationClaims.Clear();
            foreach (var claimId in claimIds)
            {
                currentUser.ApplicationClaims.Add(new ApplicationUserClaim
                {
                    ApplicationUserId = userId,
                    ApplicationClaimId = claimId
                });
            }
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public async Task<IEnumerable<ApplicationClaim>> GetUserClaimsAsync(int userId)
        {
            var currentUser = _userManager.FindById(userId);
            var claims = await this.Claims.ToListAsync();
            return claims.Where(r => currentUser.ApplicationClaims.Any(ap => ap.ApplicationClaimId == r.Id)).ToList();
        }

        public IEnumerable<ApplicationClaim> GetUserClaims(int userId)
        {
            var currentUser = _userManager.FindById(userId);
            var claims = _db.ApplicationClaim.ToList();
            return claims.Where(r => currentUser.ApplicationClaims.Any(ap => ap.ApplicationClaimId == r.Id));
        }

        public IdentityResult ClearUserGroups(int userId)
        {
            return this.SetUserClaims(userId, new int[] { });
        }

        public async Task<IdentityResult> ClearUserClaimsAsync(int userId)
        {
            return await this.SetUserClaimsAsync(userId, new int[] { });
        }

        public async Task<IdentityResult> DeleteClaimAsync(int claimId)
        {
            var claim = await this.FindByIdAsync(claimId);
            if (claim == null)
            {
                throw new ArgumentNullException("claimId");
            }
            _db.ApplicationClaim.Remove(claim);
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public IdentityResult DeleteGroup(int claimId)
        {
            var claim = this.FindById(claimId);

            if (claim == null)
            {
                throw new ArgumentNullException("claimId");
            }
            _db.ApplicationClaim.Remove(claim);
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateClaimAsync(ApplicationClaim claim)
        {
            await _claimStore.UpdateAsync(claim);
            return IdentityResult.Success;
        }

        public IdentityResult UpdateGroup(ApplicationClaim claim)
        {
            _claimStore.Update(claim);
            return IdentityResult.Success;
        }

        public async Task<ApplicationClaim> FindByIdAsync(int id)
        {
            return await _claimStore.FindByIdAsync(id);
        }

        public ApplicationClaim FindById(int id)
        {
            return _claimStore.FindById(id);
        }

        public IEnumerable<ApplicationClaim> FetchAll()
        {
            return _claimStore.Claims.ToList();
        }
        public async Task<IEnumerable<ApplicationClaim>> FetchAllAsync()
        {
            return await _claimStore.Claims.ToListAsync();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}