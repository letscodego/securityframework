using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Security.Identity
{
    public class ApplicationGroupManager
    {
        private readonly ApplicationGroupStore _groupStore;
        private readonly ApplicationDbContext _db;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationRoleManager _roleManager;
        private readonly ApplicationClaimStore _claimManager;

        public ApplicationGroupManager()
        {
            _db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            _roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            _claimManager = new ApplicationClaimStore(_db);
            _groupStore = new ApplicationGroupStore(_db);
        }
        public ApplicationGroupManager(ApplicationDbContext dbContext, ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            _db = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _claimManager = new ApplicationClaimStore(_db);
            _groupStore = new ApplicationGroupStore(_db);
        }

        public IQueryable<ApplicationGroup> Groups
        {
            get
            {
                return _groupStore.Groups;
            }
        }

        public async Task<IdentityResult> CreateGroupAsync(ApplicationGroup group)
        {
            await _groupStore.CreateAsync(group);
            return IdentityResult.Success;
        }

        public IdentityResult CreateGroup(ApplicationGroup group)
        {
            _groupStore.Create(group);
            return IdentityResult.Success;
        }

        public IdentityResult SetGroupRoles(int groupId, params int[] roleIds)
        {
            // Clear all the roles associated with this group:
            var thisGroup = this.FindById(groupId);
            thisGroup.ApplicationRoles.Clear();
            // Add the new roles passed in:
            foreach (var item in roleIds)
            {
                thisGroup.ApplicationRoles.Add(new ApplicationGroupRole
                {
                    ApplicationGroupId = groupId,
                    ApplicationRoleId = item
                });
            }
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetGroupRolesAsync(int groupId, params int[] roleIds)
        {
            // Clear all the roles associated with this group:
            var thisGroup = await this.FindByIdAsync(groupId);
            thisGroup.ApplicationRoles.Clear();
            foreach (var item in roleIds)
            {
                thisGroup.ApplicationRoles.Add(new ApplicationGroupRole
                {
                    ApplicationGroupId = groupId,
                    ApplicationRoleId = item
                });
            }
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetUserGroupsAsync(int userId, params int[] groupIds)
        {
            // Clear current group membership:           
            var currentUser = await _userManager.FindByIdAsync(userId);
            currentUser.ApplicationGroups.Clear();
            // Add the user to the new groups:
            foreach (int groupId in groupIds)
            {
                currentUser.ApplicationGroups.Add(new ApplicationUserGroup
                {
                    ApplicationUserId = userId,
                    ApplicationGroupId = groupId
                });
            }
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public IdentityResult SetUserGroups(int userId, params int[] groupIds)
        {
            // Clear current group membership:  
            var currentUser = _userManager.FindById(userId);
            currentUser.ApplicationGroups.Clear();
            // Add the user to the new groups:
            foreach (var groupId in groupIds)
            {
                currentUser.ApplicationGroups.Add(new ApplicationUserGroup
                {
                    ApplicationUserId = userId,
                    ApplicationGroupId = groupId
                });
            }
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public IdentityResult SetGroupClaims(int groupId, params int[] claimIds)
        {
            var thisGroup = this.FindById(groupId);
            thisGroup.ApplicationClaims.Clear();
            foreach (var item in claimIds)
            {
                thisGroup.ApplicationClaims.Add(new ApplicationGroupClaim
                {
                    ApplicationGroupId = groupId,
                    ApplicationClaimId = item
                });
            }
            _db.SaveChanges();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetGroupClaimsAsync(int groupId, params int[] claimIds)
        {
            var thisGroup = await this.FindByIdAsync(groupId);
            thisGroup.ApplicationClaims.Clear();
            foreach (var item in claimIds)
            {
                thisGroup.ApplicationClaims.Add(new ApplicationGroupClaim
                {
                    ApplicationGroupId = groupId,
                    ApplicationClaimId = item
                });
            }
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteGroupAsync(int groupId)
        {
            var group = await this.FindByIdAsync(groupId);
            if (group == null)
            {
                throw new ArgumentNullException("groupId");
            }
            // remove the roles from the group:
            group.ApplicationRoles.Clear();
            // Remove all the users:
            group.ApplicationUsers.Clear();
            // Remove the group itself:
            _db.ApplicationGroup.Remove(group);
            await _db.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public IdentityResult DeleteGroup(int groupId)
        {
            var group = this.FindById(groupId);
            if (group == null)
            {
                throw new ArgumentNullException("groupId");
            }
            // remove the roles from the group:
            group.ApplicationRoles.Clear();
            // Remove all the users:
            group.ApplicationUsers.Clear();
            // Remove the group itself:
            _db.ApplicationGroup.Remove(group);
            _db.SaveChanges();
            return IdentityResult.Success;
        }
        
        public async Task<IdentityResult> UpdateGroupAsync(ApplicationGroup group)
        {
            await _groupStore.UpdateAsync(group);
            return IdentityResult.Success;
        }

        public IdentityResult UpdateGroup(ApplicationGroup group)
        {
            _groupStore.Update(group);
            return IdentityResult.Success;
        }

        public IdentityResult ClearUserGroups(int userId)
        {
            return this.SetUserGroups(userId, new int[] { });
        }

        public async Task<IdentityResult> ClearUserGroupsAsync(int userId)
        {
            return await this.SetUserGroupsAsync(userId, new int[] { });
        }

        public IdentityResult ClearGroupClaims(int groupId)
        {
            return this.SetGroupClaims(groupId, new int[] { });
        }

        public async Task<IdentityResult> ClearGroupClaimsAsync(int groupId)
        {
            return await this.SetGroupClaimsAsync(groupId, new int[] { });
        }

        public async Task<IEnumerable<ApplicationGroup>> GetUserGroupsAsync(int userId)
        {
            var userGroups = (from g in this.Groups
                              where g.ApplicationUsers
                                .Any(u => u.ApplicationUserId == userId)
                              select g).ToListAsync();
            return await userGroups;
        }

        public IEnumerable<ApplicationGroup> GetUserGroups(int userId)
        {
            var userGroups = (from g in this.Groups
                              where g.ApplicationUsers
                                .Any(u => u.ApplicationUserId == userId)
                              select g).ToList();
            return userGroups;
        }

        public async Task<IEnumerable<ApplicationRole>> GetGroupRolesAsync(int groupId)
        {
            var grp = await _db.ApplicationGroup
                .FirstOrDefaultAsync(g => g.Id == groupId);
            var roles = await _roleManager.Roles.ToListAsync();
            var groupRoles = (from r in roles
                              where grp.ApplicationRoles
                                .Any(ap => ap.ApplicationRoleId == r.Id)
                              select r).ToList();
            return groupRoles;
        }

        public IEnumerable<ApplicationRole> GetGroupRoles(int groupId)
        {
            var grp = _db.ApplicationGroup.FirstOrDefault(g => g.Id == groupId);
            var roles = _roleManager.Roles.ToList();
            var groupRoles = from r in roles
                             where grp.ApplicationRoles
                                .Any(ap => ap.ApplicationRoleId == r.Id)
                             select r;
            return groupRoles;
        }

        public async Task<IEnumerable<ApplicationClaim>> GetGroupClaimsAsync(int groupId)
        {
            var grp = await _db.ApplicationGroup.FirstOrDefaultAsync(g => g.Id == groupId);
            var claims = await _claimManager.Claims.ToListAsync();
            var groupClaims = (from r in claims
                              where grp.ApplicationClaims
                                .Any(ap => ap.ApplicationClaimId== r.Id)
                              select r).ToList();
            return groupClaims;
        }

        public IEnumerable<ApplicationClaim> GetGroupClaims(int groupId)
        {
            var grp = _db.ApplicationGroup.FirstOrDefault(g => g.Id == groupId);
            var claims = _db.ApplicationClaim.ToList();
            var groupClaims = from r in claims
                             where grp.ApplicationClaims
                                .Any(ap => ap.ApplicationClaimId == r.Id)
                             select r;
            return groupClaims;
        }

        public IEnumerable<ApplicationUser> GetGroupUsers(int groupId)
        {
            var group = this.FindById(groupId);
            var users = new List<ApplicationUser>();
            foreach (var groupUser in group.ApplicationUsers)
            {
                var user = _db.Users.Find(groupUser.ApplicationUserId);
                users.Add(user);
            }
            return users;
        }

        //public async Task<IEnumerable<ApplicationUser>> GetGroupUsersAsync(int groupId)
        //{
        //    var group = await this.FindByIdAsync(groupId);

        //    var users = new List<ApplicationUser>();

        //    foreach (var groupUser in group.ApplicationUsers)
        //    {

        //        var user = await _db.Users

        //            .FirstOrDefaultAsync(u => u.Id == groupUser.ApplicationUserId);

        //        users.Add(user);

        //    }

        //    return users;
        //}

        //public IEnumerable<ApplicationGroupRole> GetUserGroupRoles(int userId)
        //{
        //    var userGroups = this.GetUserGroups(userId);

        //    var userGroupRoles = new List<ApplicationGroupRole>();

        //    foreach (var group in userGroups)
        //    {

        //        userGroupRoles.AddRange(group.ApplicationRoles.ToArray());

        //    }

        //    return userGroupRoles;
        //}

        //public async Task<IEnumerable<ApplicationGroupRole>> GetUserGroupRolesAsync(int userId)
        //{
        //    var userGroups = await this.GetUserGroupsAsync(userId);

        //    var userGroupRoles = new List<ApplicationGroupRole>();

        //    foreach (var group in userGroups)
        //    {

        //        userGroupRoles.AddRange(group.ApplicationRoles.ToArray());

        //    }

        //    return userGroupRoles;
        //}

        public async Task<ApplicationGroup> FindByIdAsync(int id)
        {
            return await _groupStore.FindByIdAsync(id);
        }

        public ApplicationGroup FindById(int id)
        {
            return _groupStore.FindById(id);
        }
    }
}