using System.Collections.Generic;
using System.Threading.Tasks;
using APIWithIdentity.DomainModel;
using APIWithIdentity.Persistence.Repository.Contracts;

namespace APIWithIdentity.Persistence.Repository
{
    public class Repository<TEntity>  : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;

        public Repository(AppDbContext context)
        {
            this.Context = context;
        }
        
        public async Task AddAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }
    }
}