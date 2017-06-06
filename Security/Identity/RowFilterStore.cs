using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Identity
{
    public class RowFilterStore : IDisposable
    {
        private bool _disposed;
        private RowFilterStoreBase _rowFilterStoreBase;
        private RowFilterTypeStoreBase _rowFilterTypeStoreBase;

        public RowFilterStore(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.Context = context;
            this._rowFilterStoreBase = new RowFilterStoreBase(context);
            this._rowFilterTypeStoreBase = new RowFilterTypeStoreBase(context);
        }

        public IQueryable<ApplicationPrincipalRowFilter> RowFilters
        {
            get
            {
                return this._rowFilterStoreBase.EntitySet;
            }
        }

        public IQueryable<ApplicationRowFilterType> RowFilterTypes
        {
            get
            {
                return this._rowFilterTypeStoreBase.EntitySet;
            }
        }

        public DbContext Context
        {
            get;
            private set;
        }

        public virtual void Create(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Create(rowFilter);
            this.Context.SaveChanges();
        }

        public virtual async Task CreateAsync(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Create(rowFilter);
            await this.Context.SaveChangesAsync();

        }

        public Task<ApplicationPrincipalRowFilter> FindByIdAsync(int rowFilterId)
        {
            this.ThrowIfDisposed();
            return this._rowFilterStoreBase.GetByIdAsync(rowFilterId);
        }

        public ApplicationPrincipalRowFilter FindById(int rowFilterId)
        {
            this.ThrowIfDisposed();
            return this._rowFilterStoreBase.GetById(rowFilterId);
        }

        public virtual async Task DeleteAsync(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Delete(rowFilter);
            await this.Context.SaveChangesAsync();
        }

        public virtual void Delete(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Delete(rowFilter);
            this.Context.SaveChanges();
        }

        public virtual async Task UpdateAsync(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Update(rowFilter);
            await this.Context.SaveChangesAsync();

        }

        public virtual void Update(ApplicationPrincipalRowFilter rowFilter)
        {
            this.ThrowIfDisposed();
            if (rowFilter == null)
            {
                throw new ArgumentNullException("rowFilter");
            }
            this._rowFilterStoreBase.Update(rowFilter);
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
            this._rowFilterStoreBase = null;
        }
    }
}