using AutoMapper;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;

namespace CarMS_API.Models.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();

            CreateMap<Car, CarDto>()
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
            .ReverseMap();
            CreateMap<Car, CarCreateDto>().ReverseMap();
            CreateMap<Car, CarUpdateDto>().ReverseMap();

            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, BrandCreateDto>().ReverseMap();
            CreateMap<Brand, BrandUpdateDto>().ReverseMap();

            CreateMap<Seller, SellerDto>()
           .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User)).ReverseMap();
            CreateMap<Seller, SellerCreateDto>().ReverseMap();

            CreateMap<CarHistory, CarHistoryCreateDto>().ReverseMap();
            CreateMap<CarHistory, CarHistoryDto>().ReverseMap();

            CreateMap<CarMaintenance, CarMaintenanceCreateDto>().ReverseMap();
            CreateMap<CarMaintenance, CarMaintenanceDto>().ReverseMap();

            CreateMap<Approval, ApprovalDto>().ReverseMap();
            CreateMap<Approval, ApprovalCreateDto>().ReverseMap();

            CreateMap<TestDrive, TestDriveDto>().ReverseMap();
            CreateMap<TestDrive, TestDriveCreateDto>().ReverseMap();

            CreateMap<Reservation, ReservationDto>().ReverseMap();
            CreateMap<Reservation, ReservationCreateDto>().ReverseMap();
        }
    }
}
