using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class TestDriveSearchRepository : ISearchableRepository<TestDrive, TestDriveSearchParams>
    {
        public Expression<Func<TestDrive, bool>> BuildFilter(TestDriveSearchParams p)
        {
            return td =>
                (string.IsNullOrEmpty(p.UserId) || td.UserId == p.UserId) &&
                (!p.CarId.HasValue || td.CarId == p.CarId.Value) &&
                (!p.Status.HasValue || td.StatusTestDrive == p.Status.Value) &&
                (!p.AppointmentFrom.HasValue || td.AppointmentDate >= p.AppointmentFrom.Value) &&
                (!p.AppointmentTo.HasValue || td.AppointmentDate <= p.AppointmentTo.Value);
        }

        public Func<IQueryable<TestDrive>, IOrderedQueryable<TestDrive>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "userid" => q => q.OrderBy(td => td.UserId),
                "userid_desc" => q => q.OrderByDescending(td => td.UserId),

                "appointmentdate" => q => q.OrderBy(td => td.AppointmentDate),
                "appointmentdate_desc" => q => q.OrderByDescending(td => td.AppointmentDate),

                "status" => q => q.OrderBy(td => td.StatusTestDrive),
                "status_desc" => q => q.OrderByDescending(td => td.StatusTestDrive),

                _ => q => q.OrderBy(td => td.Id)
            };
        }

        public Func<IQueryable<TestDrive>, IQueryable<TestDrive>> Include()
        {
            return q => q
                .Include(td => td.User)
                .Include(td => td.Car);
        }
    }

}
