using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarMS_API.Repositorys
{
    public class CarSearchRepository : ISearchableRepository<Car, CarSearchParams>
    {
        public Expression<Func<Car, bool>> BuildFilter(CarSearchParams p)
        {
            return c =>
                !c.IsDeleted &&
                (string.IsNullOrEmpty(p.SearchTerm) ||
                    c.Model.Contains(p.SearchTerm) ||
                    c.Description.Contains(p.SearchTerm)) &&
                (!p.MinPrice.HasValue || c.Price >= p.MinPrice) &&
                (!p.MaxPrice.HasValue || c.Price <= p.MaxPrice) &&
                (!p.MinYear.HasValue || c.Year >= p.MinYear) &&
                (!p.MaxYear.HasValue || c.Year <= p.MaxYear) &&
                (!p.MinMileage.HasValue || c.Mileage >= p.MinMileage) &&
                (!p.MaxMileage.HasValue || c.Mileage <= p.MaxMileage) &&
                (string.IsNullOrEmpty(p.Color) || c.Color.ToLower().Contains(p.Color.ToLower())) &&
                (!p.EngineType.HasValue || c.EngineType == p.EngineType) &&
                (!p.GearType.HasValue || c.GearType == p.GearType) &&
                (!p.CarType.HasValue || c.CarType == p.CarType) &&
                (!p.IsUsed.HasValue || c.IsUsed == p.IsUsed) &&
                (!p.Status.HasValue || c.Status == p.Status) &&
                (!p.SellerId.HasValue || c.SellerId == p.SellerId) &&
                (!p.BrandId.HasValue || c.BrandId == p.BrandId) &&
                (!p.IsApproved.HasValue || c.IsApproved == p.IsApproved) &&
                (string.IsNullOrEmpty(p.CarRegistrationNumber) || c.CarRegistrationNumber.Contains(p.CarRegistrationNumber)) &&
                (string.IsNullOrEmpty(p.CarIdentificationNumber) || c.CarIdentificationNumber.Contains(p.CarIdentificationNumber)) &&
                (string.IsNullOrEmpty(p.EngineNumber) || c.EngineNumber.Contains(p.EngineNumber));
        }

        public Func<IQueryable<Car>, IOrderedQueryable<Car>> BuildSort(string? sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "price" => q => q.OrderBy(c => c.Price),
                "price_desc" => q => q.OrderByDescending(c => c.Price),
                "year" => q => q.OrderByDescending(c => c.Year),
                "createdat" => q => q.OrderByDescending(c => c.CreatedAt),
                "updatedat" => q => q.OrderByDescending(c => c.UpdatedAt),
                "isapproved" => q => q.OrderBy(c => c.IsApproved),
                "carregistrationnumber" => q => q.OrderBy(c => c.CarRegistrationNumber),
                _ => q => q.OrderBy(c => c.Id)
            };
        }


        public Func<IQueryable<Car>, IQueryable<Car>> Include()
        {
            return q => q.Include(c => c.Brand)
                         .Include(c => c.Seller);
        }
    }
}
