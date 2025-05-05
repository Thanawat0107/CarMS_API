using AutoMapper;
using CarMS_API.Models.Dto;

namespace CarMS_API.Models.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Car, CarDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Seller, SellerDto>().ReverseMap();
        }
    }
}
