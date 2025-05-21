using AutoMapper;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;

namespace CarMS_API.Models.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Car, CarDto>().ReverseMap();
            CreateMap<Car, CarCreateDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Seller, SellerDto>().ReverseMap();
            CreateMap<CarHistory, CarHistoryCreateDto>().ReverseMap();
            CreateMap<CarHistory, CarHistoryDto>().ReverseMap();
            CreateMap<CarMaintenance, CarMaintenanceCreateDto>().ReverseMap();
            CreateMap<CarMaintenance, CarMaintenanceDto>().ReverseMap();
            CreateMap<Approval, ApprovalDto>().ReverseMap();
            CreateMap<Approval, ApprovalCreateDto>().ReverseMap();
            CreateMap<TestDrive, TestDriveDto>().ReverseMap();
            CreateMap<TestDrive, TestDriveCreateDto>().ReverseMap();
        }
    }
}
