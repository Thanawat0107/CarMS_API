using AutoMapper;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Dto.UpdateDto;

namespace CarMS_API.Models.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();

            CreateMap<Car, CarDto>()
                // ไม่ต้องเขียน ForMember แมปรูประหว่าง FileList เพราะใน Car กับ CarDto เราตั้งชื่อว่า CarImages เหมือนกันเป๊ะแล้ว
                .ReverseMap();
            // 2. สร้างข้อมูลใหม่ (Create)
            CreateMap<CarCreateDto, Car>()
                // มองข้ามฟิลด์ NewImages (ที่เป็นไฟล์) ไปเลย เดี๋ยวเราไปลูปเซฟรูปเองใน Controller
                .ForSourceMember(src => src.NewImages, opt => opt.DoNotValidate());
            // 3. อัปเดตข้อมูล (Update)
            CreateMap<CarUpdateDto, Car>()
                // มองข้ามฟิลด์รูปภาพทั้งหมด เพราะเราต้องจัดการ KeepImages และ NewImages เอง
                .ForMember(dest => dest.CarImages, opt => opt.Ignore())
                // 🌟 ป้องกันไม่ให้ค่า null จาก DTO ไปทับข้อมูลเดิมในตาราง
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Approval ถูกรวมเข้า Car แล้ว — map จาก Car โดยเอา Id → CarId
            CreateMap<Car, ApprovalDto>()
                .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src));
            CreateMap<ApprovalCreateDto, Car>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CarId))
                .ForMember(dest => dest.ApprovalRemark, opt => opt.MapFrom(src => src.Remark));

            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<Brand, BrandCreateDto>().ReverseMap();
            CreateMap<Brand, BrandUpdateDto>().ReverseMap();

            CreateMap<Seller, SellerDto>()
           .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User)).ReverseMap();
            CreateMap<Seller, SellerCreateDto>().ReverseMap();

            CreateMap<CarMaintenance, CarMaintenanceCreateDto>().ReverseMap();
            CreateMap<CarMaintenance, CarMaintenanceDto>()
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
                .ReverseMap();

            CreateMap<TestDrive, TestDriveDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
                .ReverseMap();
            CreateMap<TestDrive, TestDriveCreateDto>().ReverseMap();

            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
                .ReverseMap();
            CreateMap<Booking, BookingCreateDto>().ReverseMap();

            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Booking, opt => opt.MapFrom(src => src.Booking))
                .ReverseMap();
            CreateMap<Payment, PaymentCreateDto>().ReverseMap();
            CreateMap<Payment, PaymentUpdateDto>().ReverseMap();

            // แมปสำหรับการดึงข้อความแชทมาแสดง
            CreateMap<ChatMessage, ChatMessageDto>().ReverseMap();
            // แมปสำหรับการรับค่าเพื่อบันทึกลง Database
            CreateMap<ChatMessageCreateDto, ChatMessage>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ReverseMap();
            CreateMap<ReviewCreateDto, Review>();
            CreateMap<ReviewUpdateDto, Review>();

            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Car, opt => opt.MapFrom(src => src.Car))
                .ReverseMap();
            CreateMap<LoanCreateDto, Loan>();
            CreateMap<LoanUpdateDto, Loan>();
        }
    }
}
