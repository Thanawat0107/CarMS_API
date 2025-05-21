using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class CarMaintenanceSearchRepository : ISearchableRepository<CarMaintenance, CarMaintenanceSearchParams>
    {
        public Expression<Func<CarMaintenance, bool>> BuildFilter(CarMaintenanceSearchParams p)
        {
            return cm =>
                !cm.IsDeleted &&
                (!p.CarHistoryId.HasValue || cm.CarHistoryId == p.CarHistoryId) &&
                (string.IsNullOrEmpty(p.Title) || cm.Title.Contains(p.Title)) &&
                (string.IsNullOrEmpty(p.Description) || cm.Description.Contains(p.Description)) &&
                (!p.MinCost.HasValue || cm.TentativelyCost >= p.MinCost.Value) &&
                (!p.MaxCost.HasValue || cm.TentativelyCost <= p.MaxCost.Value) &&
                (!p.ServiceDateFrom.HasValue || cm.ServiceDate >= p.ServiceDateFrom.Value) &&
                (!p.ServiceDateTo.HasValue || cm.ServiceDate <= p.ServiceDateTo.Value) &&
                (!p.IsUsed.HasValue || cm.IsUsed == p.IsUsed);
        }

        public Func<IQueryable<CarMaintenance>, IOrderedQueryable<CarMaintenance>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "carhistoryid" => q => q.OrderBy(cm => cm.CarHistoryId),
                "carhistoryid_desc" => q => q.OrderByDescending(cm => cm.CarHistoryId),
                "serviceDate" => q => q.OrderBy(cm => cm.ServiceDate),
                "serviceDate_desc" => q => q.OrderByDescending(cm => cm.ServiceDate),
                _ => q => q.OrderBy(cm => cm.Id)
            };
        }

        public Func<IQueryable<CarMaintenance>, IQueryable<CarMaintenance>> Include()
        {
            return q => q.Include(cm => cm.CarHistory).ThenInclude(cm => cm.Car);
        }
    }
}
