using CarMS_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys.IRepositorys
{
    public interface ISearchableRepository<T, TSearchParams>
    {
        Expression<Func<T, bool>> BuildFilter(TSearchParams searchParams);
        Func<IQueryable<T>, IOrderedQueryable<T>> BuildSort(string? sortBy);
        Func<IQueryable<T>, IQueryable<T>> Include();
    }
}
