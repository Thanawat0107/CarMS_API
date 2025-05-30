using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class ReservationSearchRepository : ISearchableRepository<Reservation, ReservationSearchParams>
    {
        public Expression<Func<Reservation, bool>> BuildFilter(ReservationSearchParams p)
        {
            return r =>
                (string.IsNullOrEmpty(p.UserName) || r.User.UserName.Contains(p.UserName)) &&
                (string.IsNullOrEmpty(p.CarSearchTerm) ||
                    r.Car.Model.Contains(p.CarSearchTerm)) &&
                (!p.Status.HasValue || r.Status == p.Status.Value) &&
                (!p.ReservedAtFrom.HasValue || r.ReservedAt >= p.ReservedAtFrom.Value) &&
                (!p.ReservedAtTo.HasValue || r.ReservedAt <= p.ReservedAtTo.Value) &&
                (!p.ExpiryAtFrom.HasValue || r.ExpiryAt >= p.ExpiryAtFrom.Value) &&
                (!p.ExpiryAtTo.HasValue || r.ExpiryAt <= p.ExpiryAtTo.Value);
        }

        public Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "userid" => q => q.OrderBy(r => r.UserId),
                "userid_desc" => q => q.OrderByDescending(r => r.UserId),

                "username" => q => q.OrderBy(r => r.User.UserName),
                "username_desc" => q => q.OrderByDescending(r => r.User.UserName),

                "carid" => q => q.OrderBy(r => r.CarId),
                "carid_desc" => q => q.OrderByDescending(r => r.CarId),

                "carname" => q => q.OrderBy(r => r.Car.Brand.Name),
                "carname_desc" => q => q.OrderByDescending(r => r.Car.Brand.Name),

                "reservedat" => q => q.OrderBy(r => r.ReservedAt),
                "reservedat_desc" => q => q.OrderByDescending(r => r.ReservedAt),

                "expiryat" => q => q.OrderBy(r => r.ExpiryAt),
                "expiryat_desc" => q => q.OrderByDescending(r => r.ExpiryAt),

                "status" => q => q.OrderBy(r => r.Status),
                "status_desc" => q => q.OrderByDescending(r => r.Status),

                _ => q => q.OrderBy(r => r.Id)
            };
        }

        public Func<IQueryable<Reservation>, IQueryable<Reservation>> Include()
        {
            return q => q
                .Include(r => r.User)
                .Include(r => r.Car);
        }
    }
}