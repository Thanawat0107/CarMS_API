using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class CarHistorySearchRepository : ISearchableRepository<CarHistory, CarHistorySearchParams>
    {
        public Expression<Func<CarHistory, bool>> BuildFilter(CarHistorySearchParams p)
        {
            return ch =>
                (!p.CarId.HasValue || ch.CarId == p.CarId) &&
                (string.IsNullOrEmpty(p.Detail) || ch.Detail.Contains(p.Detail)) &&
                (string.IsNullOrEmpty(p.Insurance) || ch.Insurance.Contains(p.Insurance)) &&
                (string.IsNullOrEmpty(p.Act) || ch.Act.Contains(p.Act)) &&
                (string.IsNullOrEmpty(p.Finance) || ch.Finance.Contains(p.Finance)) &&
                (string.IsNullOrEmpty(p.Source) || ch.Source.Contains(p.Source)) &&

                (!p.IsCollisionHistory.HasValue || ch.IsCollisionHistory == p.IsCollisionHistory.Value) &&
                (!p.IsApproved.HasValue || ch.IsApproved == p.IsApproved.Value) &&
                (!p.IsUsed.HasValue || ch.IsUsed == p.IsUsed.Value) &&
                (!p.IsDeleted.HasValue || ch.IsDeleted == p.IsDeleted.Value) &&

                (!p.CreatedFrom.HasValue || ch.CreatedAt >= p.CreatedFrom.Value) &&
                (!p.CreatedTo.HasValue || ch.CreatedAt <= p.CreatedTo.Value);
        }

        public Func<IQueryable<CarHistory>, IOrderedQueryable<CarHistory>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "carid" => q => q.OrderBy(ch => ch.CarId),
                "carid_desc" => q => q.OrderByDescending(ch => ch.CarId),
                "createdat" => q => q.OrderBy(ch => ch.CreatedAt),
                "createdat_desc" => q => q.OrderByDescending(ch => ch.CreatedAt),
                "updatedat" => q => q.OrderBy(ch => ch.UpdatedAt),
                "updatedat_desc" => q => q.OrderByDescending(ch => ch.UpdatedAt),
                _ => q => q.OrderBy(ch => ch.Id)
            };
        }

        public Func<IQueryable<CarHistory>, IQueryable<CarHistory>> Include()
        {
            return q => q.Include(ch => ch.Car);
        }
    }

}
