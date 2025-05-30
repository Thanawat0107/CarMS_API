using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class SellerSearchRepository : ISearchableRepository<Seller, SellerSearchParams>
    {
        public Expression<Func<Seller, bool>> BuildFilter(SellerSearchParams p)
        {
            return s =>
                (string.IsNullOrEmpty(p.UserSearchTerm) ||
                s.User.FullName.Contains(p.UserSearchTerm) ||
                s.User.Email.Contains(p.UserSearchTerm) ||
                s.User.UserName.Contains(p.UserSearchTerm) ||
                s.User.PhoneNumber.Contains(p.UserSearchTerm)) &&

                (string.IsNullOrEmpty(p.IdentityNumber) || s.IdentityNumber.Contains(p.IdentityNumber)) &&
                (string.IsNullOrEmpty(p.Address) || s.Address.Contains(p.Address)) &&
                (!p.IsVerified.HasValue || s.IsVerified == p.IsVerified);
        }

        public Func<IQueryable<Seller>, IOrderedQueryable<Seller>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "userid" => q => q.OrderBy(s => s.UserId),
                "userid_desc" => q => q.OrderByDescending(s => s.UserId),
                "identitynumber" => q => q.OrderBy(s => s.IdentityNumber),
                "identitynumber_desc" => q => q.OrderByDescending(s => s.IdentityNumber),
                "isverified" => q => q.OrderBy(s => s.IsVerified),
                "isverified_desc" => q => q.OrderByDescending(s => s.IsVerified),
                _ => q => q.OrderBy(s => s.Id)
            };
        }

        public Func<IQueryable<Seller>, IQueryable<Seller>> Include()
        {
            return q => q.Include(q=>q.User);
        }
    }

}
