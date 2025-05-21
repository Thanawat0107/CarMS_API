using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class ApprovalSearchRepository : ISearchableRepository<Approval, ApprovalSearchParams>
    {
        public Expression<Func<Approval, bool>> BuildFilter(ApprovalSearchParams p)
        {
            return a =>
                (string.IsNullOrEmpty(p.UserId) || a.UserId == p.UserId) &&
                (!p.CarHistoryId.HasValue || a.CarHistoryId == p.CarHistoryId.Value) &&
                (string.IsNullOrEmpty(p.Remark) || a.Remark.Contains(p.Remark)) &&
                (!p.ApprovedFrom.HasValue || a.ApprovedAt >= p.ApprovedFrom.Value) &&
                (!p.ApprovedTo.HasValue || a.ApprovedAt <= p.ApprovedTo.Value) &&
                (!p.ApprovalStatus.HasValue || a.ApprovalStatus == p.ApprovalStatus.Value);
        }

        public Func<IQueryable<Approval>, IOrderedQueryable<Approval>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "userid" => q => q.OrderBy(a => a.UserId),
                "userid_desc" => q => q.OrderByDescending(a => a.UserId),
                "approvedat" => q => q.OrderBy(a => a.ApprovedAt),
                "approvedat_desc" => q => q.OrderByDescending(a => a.ApprovedAt),
                _ => q => q.OrderBy(a => a.Id)
            };
        }

        public Func<IQueryable<Approval>, IQueryable<Approval>> Include()
        {
            return q => q
                .Include(a => a.User)
                .Include(a => a.CarHistory).ThenInclude(a => a.Car);
        }
    }

}
