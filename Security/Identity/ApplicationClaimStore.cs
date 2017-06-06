using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Identity
{
    public class ApplicationClaimStore : IDisposable
    {
        private bool _disposed;
        private ClaimStoreBase _claimStore;

        public ApplicationClaimStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.Context = context;
            this._claimStore = new ClaimStoreBase(context);
        }

        public IQueryable<ApplicationClaim> Claims
        {
            get
            {
                return this._claimStore.EntitySet;
            }
        }

        public DbContext Context
        {
            get;
            private set;
        }

        public virtual void Create(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Create(claim);
            this.Context.SaveChanges();
        }

        public virtual async Task CreateAsync(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Create(claim);
            await this.Context.SaveChangesAsync();

        }

        public virtual async Task DeleteAsync(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Delete(claim);
            await this.Context.SaveChangesAsync();
        }

        public virtual void Delete(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Delete(claim);
            this.Context.SaveChanges();
        }

        public Task<ApplicationClaim> FindByIdAsync(int groupId)
        {
            this.ThrowIfDisposed();
            return this._claimStore.GetByIdAsync(groupId);
        }

        public ApplicationClaim FindById(int groupId)
        {
            this.ThrowIfDisposed();
            return this._claimStore.GetById(groupId);
        }

        public Task<ApplicationClaim> FindByNameAsync(string claimName)
        {
            this.ThrowIfDisposed();
            return QueryableExtensions
                .FirstOrDefaultAsync<ApplicationClaim>(this._claimStore.EntitySet,
                    (ApplicationClaim u) => u.Key.ToUpper() == claimName.ToUpper());
        }

        public virtual async Task UpdateAsync(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Update(claim);
            await this.Context.SaveChangesAsync();

        }

        public virtual void Update(ApplicationClaim claim)
        {
            this.ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }
            this._claimStore.Update(claim);
            this.Context.SaveChanges();
        }

        // DISPOSE STUFF: ===============================================
        public bool DisposeContext { get; set; }
        private void ThrowIfDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.DisposeContext && disposing && this.Context != null)
            {
                this.Context.Dispose();
            }
            this._disposed = true;
            this.Context = null;
            this._claimStore = null;
        }
    }
}