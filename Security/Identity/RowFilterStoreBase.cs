using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Identity
{
    public class RowFilterStoreBase
    {
        public DbContext Context { get; private set; }
        public DbSet<ApplicationPrincipalRowFilter> DbEntitySet { get; private set; }
        public IQueryable<ApplicationPrincipalRowFilter> EntitySet
        {
            get
            {
                return this.DbEntitySet;
            }
        }

        public RowFilterStoreBase(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<ApplicationPrincipalRowFilter>();
        }
        public void Create(ApplicationPrincipalRowFilter entity)
        {
            this.DbEntitySet.Add(entity);
        }

        public void Delete(ApplicationPrincipalRowFilter entity)
        {
            this.DbEntitySet.Remove(entity);
        }

        public virtual Task<ApplicationPrincipalRowFilter> GetByIdAsync(object id)
        {
            return this.DbEntitySet.FindAsync(new object[] { id });
        }

        public virtual ApplicationPrincipalRowFilter GetById(object id)
        {
            return this.DbEntitySet.Find(new object[] { id });
        }

        public virtual void Update(ApplicationPrincipalRowFilter entity)
        {
            if (entity != null)
            {
                this.Context.Entry<ApplicationPrincipalRowFilter>(entity).State = EntityState.Modified;
            }
        }

    }
}