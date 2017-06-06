using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Identity
{
    public class ClaimStoreBase
    {
        public DbContext Context { get; private set; }
        public DbSet<ApplicationClaim> DbEntitySet { get; private set; }
        public IQueryable<ApplicationClaim> EntitySet
        {
            get
            {
                return this.DbEntitySet;
            }
        }

        public ClaimStoreBase(DbContext context)
        {
            this.Context = context;
            this.DbEntitySet = context.Set<ApplicationClaim>();
        }

        public void Create(ApplicationClaim entity)
        {
            this.DbEntitySet.Add(entity);
        }

        public void Delete(ApplicationClaim entity)
        {
            this.DbEntitySet.Remove(entity);
        }

        public virtual Task<ApplicationClaim> GetByIdAsync(object id)
        {
            return this.DbEntitySet.FindAsync(new object[] { id });
        }

        public virtual ApplicationClaim GetById(object id)
        {
            return this.DbEntitySet.Find(new object[] { id });
        }

        public virtual void Update(ApplicationClaim entity)
        {
            if (entity != null)
            {
                this.Context.Entry<ApplicationClaim>(entity).State = EntityState.Modified;
            }
        }
    }
}