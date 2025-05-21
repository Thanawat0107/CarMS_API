using System.Linq.Expressions;

namespace CarMS_API.Repositorys.IRepositorys
{
    public interface IRepository<T> where T : class
    {
        public Task<(IEnumerable<T> Results, int TotalCount)> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            int pageNumber = 0,
            int pageSize = 0
        );
        public Task<T> GetByIdAsync(
            int id,
            Func<IQueryable<T>, IQueryable<T>>? include = null
        );
        public Task<T> AddAsync(T entity);
        public Task<T> UpdateAsync(T entity);
        public Task<T> DeleteAsync(int id);
    }
}
