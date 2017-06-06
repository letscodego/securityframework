using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Security.Identity
{
    public class ApplicationUserLogin : IdentityUserLogin<int> { }
    public class UserClaim : IdentityUserClaim<int> { }
    public class ApplicationUserRole : IdentityUserRole<int> { }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, UserClaim>
    {
        public ApplicationDbContext()
            : base("security")
        {
        }

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // Add the ApplicationGroups, claims,... properties:
        public virtual IDbSet<ApplicationGroup> ApplicationGroup { get; set; }
        public virtual IDbSet<ApplicationClaim> ApplicationClaim { get; set; }
        public virtual IDbSet<ApplicationPrincipalRowFilter> ApplicationPrincipalRowFilter { get; set; }
        public virtual IDbSet<ApplicationRowFilterType> ApplicationRowFilterType { get; set; }
        // Override OnModelsCreating:
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // Make sure to call the base method first:
            base.OnModelCreating(modelBuilder);
            // Map Users to Groups:
            modelBuilder.Entity<ApplicationGroup>()
                .HasMany<ApplicationUserGroup>((ApplicationGroup g) => g.ApplicationUsers)
                .WithRequired()
                .HasForeignKey<int>((ApplicationUserGroup ag) => ag.ApplicationGroupId);

            modelBuilder.Entity<ApplicationUserGroup>().HasKey((ApplicationUserGroup r) =>
                    new
                    {
                        ApplicationUserId = r.ApplicationUserId,
                        ApplicationGroupId = r.ApplicationGroupId
                    }
                    ).ToTable("ApplicationUserGroups");

            // Map Roles to Groups:
            modelBuilder.Entity<ApplicationGroup>()
                .HasMany<ApplicationGroupRole>((ApplicationGroup g) => g.ApplicationRoles)
                .WithRequired()
                .HasForeignKey<int>((ApplicationGroupRole ap) => ap.ApplicationGroupId);

            modelBuilder.Entity<ApplicationGroupRole>().HasKey((ApplicationGroupRole gr) =>
                new
                {
                    ApplicationRoleId = gr.ApplicationRoleId,
                    ApplicationGroupId = gr.ApplicationGroupId
                }
                ).ToTable("ApplicationGroupRoles");

            // Map Claims to Groups:
            modelBuilder.Entity<ApplicationGroup>()
                .HasMany<ApplicationGroupClaim>((ApplicationGroup g) => g.ApplicationClaims)
                .WithRequired()
                .HasForeignKey<int>((ApplicationGroupClaim gc) => gc.ApplicationGroupId);
            modelBuilder.Entity<ApplicationGroupClaim>().HasKey((ApplicationGroupClaim gc) =>
                new
                {
                    ApplicationClaimId = gc.ApplicationClaimId,
                    ApplicationGroupId = gc.ApplicationGroupId
                }
                ).ToTable("ApplicationGroupClaims");

            // Map Claims to Users:
            modelBuilder.Entity<ApplicationUser>()
                .HasMany<ApplicationUserClaim>((ApplicationUser g) => g.ApplicationClaims)
                .WithRequired()
                .HasForeignKey<int>((ApplicationUserClaim gc) => gc.ApplicationUserId);
            modelBuilder.Entity<ApplicationUserClaim>().HasKey((ApplicationUserClaim gc) =>
                new
                {
                    ApplicationClaimId = gc.ApplicationClaimId,
                    ApplicationUserId = gc.ApplicationUserId
                }
                ).ToTable("ApplicationUserClaims");
            modelBuilder.Entity<ApplicationGroup>()
                .HasMany(c => c.ApplicationPrincipalRowFilters)
                .WithRequired()
                .HasForeignKey(n => n.PrincipalId);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(o => o.ApplicationPrincipalRowFilters)
                .WithRequired()
                .HasForeignKey(n => n.PrincipalId);
            modelBuilder.Entity<ApplicationPrincipalRowFilter>()
                .HasRequired<ApplicationRowFilterType>(o => o.ApplicationRowFilterType)
                .WithMany(n => n.ApplicationPrincipalRowFilters);
        }
    }
    public class ApplicationUser : IdentityUser<int, ApplicationUserLogin, ApplicationUserRole, UserClaim>, IUser<int>
    {
        public ApplicationUser()
        {
            ApplicationGroups = new List<ApplicationUserGroup>();
            ApplicationClaims = new List<ApplicationUserClaim>();
            ApplicationPrincipalRowFilters = new List<ApplicationPrincipalRowFilter>();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
        public virtual ICollection<ApplicationUserGroup> ApplicationGroups { get; set; }
        public virtual ICollection<ApplicationUserClaim> ApplicationClaims { get; set; }
        public virtual ICollection<ApplicationPrincipalRowFilter> ApplicationPrincipalRowFilters { get; set; }
        public bool IsActiveDirectoryUser { get; set; }
    }
    public class ApplicationRole : IdentityRole<int, ApplicationUserRole>, IRole<int>
    {
        public string Description { get; set; }
        public ApplicationRole() { }
        public ApplicationRole(string name)
            : this()
        {
            Name = name;
        }
        public ApplicationRole(string name, string description)
            : this(name)
        {
            Description = description;
        }
    }
    public class ApplicationGroup
    {
        public ApplicationGroup()
        {
            ApplicationRoles = new List<ApplicationGroupRole>();
            ApplicationUsers = new List<ApplicationUserGroup>();
            ApplicationClaims = new List<ApplicationGroupClaim>();
            ApplicationPrincipalRowFilters = new List<ApplicationPrincipalRowFilter>();
        }

        public ApplicationGroup(string name)
            : this()
        {
            Name = name;
        }
        public ApplicationGroup(string name, string description)
            : this(name)
        {
            Description = description;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ApplicationGroupRole> ApplicationRoles { get; set; }
        public virtual ICollection<ApplicationUserGroup> ApplicationUsers { get; set; }
        public virtual ICollection<ApplicationGroupClaim> ApplicationClaims { get; set; }
        public virtual ICollection<ApplicationPrincipalRowFilter> ApplicationPrincipalRowFilters { get; set; }
    }
    public class ApplicationClaim
    {
        public ApplicationClaim()
        {
        }
        public ApplicationClaim(string name)
            : this()
        {
            ExpireDate = DateTime.Now.AddYears(1);
        }
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpireDate { get; set; }
    }
    public class ApplicationPrincipalRowFilter
    {
        public ApplicationPrincipalRowFilter()
        {
        }
        public int Id { get; set; }
        public string PrincipalType { get; set; }
        public int PrincipalId { get; set; }
        public int RowFilterTypeId { get; set; }
        public int RowFilterValue { get; set; }
        [ForeignKey("RowFilterTypeId")]
        public virtual ApplicationRowFilterType ApplicationRowFilterType { get; set; }
    }
    public class ApplicationRowFilterType
    {
        public ApplicationRowFilterType()
        {
            ApplicationPrincipalRowFilters = new List<ApplicationPrincipalRowFilter>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ApplicationPrincipalRowFilter> ApplicationPrincipalRowFilters { get; set; }
    }
    public class ApplicationUserGroup
    {
        public int ApplicationUserId { get; set; }
        public int ApplicationGroupId { get; set; }
    }
    public class ApplicationGroupRole
    {
        public int ApplicationGroupId { get; set; }
        public int ApplicationRoleId { get; set; }
    }
    public class ApplicationGroupClaim
    {
        public int ApplicationGroupId { get; set; }
        public int ApplicationClaimId { get; set; }
    }
    public class ApplicationUserClaim
    {
        public int ApplicationUserId { get; set; }
        public int ApplicationClaimId { get; set; }
    }
    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, int, ApplicationUserLogin, ApplicationUserRole, UserClaim>, IUserStore<ApplicationUser, int>, IDisposable
    {
        public ApplicationUserStore()
            : this(new IdentityDbContext())
        {
            base.DisposeContext = true;
        }

        public ApplicationUserStore(DbContext context)
            : base(context)
        {
        }

    }
    public class ApplicationRoleStore : RoleStore<ApplicationRole, int, ApplicationUserRole>, IQueryableRoleStore<ApplicationRole, int>, IRoleStore<ApplicationRole, int>, IDisposable
    {
        public ApplicationRoleStore()
            : base(new IdentityDbContext())
        {
            base.DisposeContext = true;
        }

        public ApplicationRoleStore(DbContext context)
            : base(context)
        {
        }
    }
}