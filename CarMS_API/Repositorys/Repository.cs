using CarMS_API.Data;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace CarMS_API.Repositorys
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }

        public async Task<(IEnumerable<T> Results, int TotalCount)> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
         Func<IQueryable<T>, IQueryable<T>>? include = null,
        int pageNumber = 0,
        int pageSize = 0)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);

            int totalCount = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            var result = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (result, totalCount);
        }

        public async Task<T> GetByIdAsync(
            int id,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(e =>
                EF.Property<int>(e, "Id") == id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return null;
            _dbSet.Remove(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
