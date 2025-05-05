using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class BrandSearchRepository : ISearchableRepository<Brand, BrandSearchParams>
    {
        public Expression<Func<Brand, bool>> BuildFilter(BrandSearchParams p)
        {
            return b =>
                !b.IsDelete &&
                (string.IsNullOrEmpty(p.SearchTerm) || b.Name.Contains(p.SearchTerm)) &&
                (!p.IsUsed.HasValue || b.IsUsed == p.IsUsed);
        }

        public Func<IQueryable<Brand>, IOrderedQueryable<Brand>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "name" => q => q.OrderBy(b => b.Name),
                "name_desc" => q => q.OrderByDescending(b => b.Name),
                _ => q => q.OrderBy(b => b.Id)
            };
        }

        public Func<IQueryable<Brand>, IQueryable<Brand>> Include()
        {
            return q => q;
        }
    }
}
