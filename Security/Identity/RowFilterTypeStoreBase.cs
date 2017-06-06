using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Identity
{
    public class RowFilterTypeStoreBase
    {
        public DbContext Context { get; private set; }
        public DbSet<ApplicationRowFilterType> DbEntitySet { get; private set; }
        public IQueryable<ApplicationRowFilterType> EntitySet
        {
            get
            {
                return this.DbEntitySet;
            }
        }

        public RowFilterTypeStoreBase(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<ApplicationRowFilterType>();
        }
        public void Create(ApplicationRowFilterType entity)
        {
            this.DbEntitySet.Add(entity);
        }

        public void Delete(ApplicationRowFilterType entity)
        {
            this.DbEntitySet.Remove(entity);
        }

        public virtual Task<ApplicationRowFilterType> GetByIdAsync(object id)
        {
            return this.DbEntitySet.FindAsync(new object[] { id });
        }

        public virtual ApplicationRowFilterType GetById(object id)
        {
            return this.DbEntitySet.Find(new object[] { id });
        }

        public virtual void Update(ApplicationRowFilterType entity)
        {
            if (entity != null)
            {
                this.Context.Entry<ApplicationRowFilterType>(entity).State = EntityState.Modified;
            }
        }

    }
}