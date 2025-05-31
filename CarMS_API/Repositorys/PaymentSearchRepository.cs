using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class PaymentSearchRepository : ISearchableRepository<Payment, PaymentSearchParams>
    {
        public Expression<Func<Payment, bool>> BuildFilter(PaymentSearchParams p)
        {
            return payment =>
                (string.IsNullOrEmpty(p.UserName) || payment.Reservation.User.UserName.Contains(p.UserName)) &&
                (string.IsNullOrEmpty(p.TransactionRef) || payment.TransactionRef.Contains(p.TransactionRef)) &&
                (!p.Method.HasValue || payment.Method == p.Method.Value) &&
                (!p.Status.HasValue || payment.Status == p.Status.Value) &&
                (!p.PaidAtFrom.HasValue || payment.PaidAt >= p.PaidAtFrom.Value) &&
                (!p.PaidAtTo.HasValue || payment.PaidAt <= p.PaidAtTo.Value) &&
                (!p.MinTotal.HasValue || payment.TotalPrice >= p.MinTotal.Value) &&
                (!p.MaxTotal.HasValue || payment.TotalPrice <= p.MaxTotal.Value);
        }

        public Func<IQueryable<Payment>, IOrderedQueryable<Payment>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "paidat" => q => q.OrderBy(p => p.PaidAt),
                "paidat_desc" => q => q.OrderByDescending(p => p.PaidAt),

                "totalprice" => q => q.OrderBy(p => p.TotalPrice),
                "totalprice_desc" => q => q.OrderByDescending(p => p.TotalPrice),

                "method" => q => q.OrderBy(p => p.Method),
                "method_desc" => q => q.OrderByDescending(p => p.Method),

                "status" => q => q.OrderBy(p => p.Status),
                "status_desc" => q => q.OrderByDescending(p => p.Status),

                _ => q => q.OrderBy(p => p.Id),
            };
        }

        public Func<IQueryable<Payment>, IQueryable<Payment>> Include()
        {
            return q => q
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.User);
        }
    }

}
