using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly IRepository<Car> _carRepo;
        private readonly IMapper _mapper;
        private readonly IFileUpload _fileUpload;

        public CarsController(IRepository<Car> carRepo,
            IMapper mapper, 
            IFileUpload fileUpload)
        {
            _carRepo = carRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int? sellerId, bool? isApproved, int pageNumber = 1, int pageSize = 10)
        {
            var (cars, totalCount) = await _carRepo.GetAllAsync(
                filter: q => !q.IsDeleted &&
                             (!sellerId.HasValue || q.SellerId == sellerId.Value) && // กรองตามเต็นท์รถ
                             (!isApproved.HasValue || q.IsApproved == isApproved.Value), // กรองสถานะอนุมัติ
                include: query => query
                    .Include(q => q.Seller)
                    .Include(q => q.Brand),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<CarDto>>(cars);
            var meta = new PaginationMeta { TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };

            return Ok(ApiResponse<IEnumerable<CarDto>>.Success(result, "โหลดรายการรถเรียบร้อย", meta));
        }

        [HttpGet("getbyid/{carId}")]
        public async Task<IActionResult> GetById(int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId, 
                q => q.Include(q => q.Seller)
                      .Include(q => q.Brand));

            // 🌟 เช็คด้วยว่ารถถูกลบไปแล้วหรือยัง
            if (car == null || car.IsDeleted) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณค้นหา หรือรถถูกลบไปแล้ว"));
                
            var result = _mapper.Map<CarDto>(car);
            return Ok(ApiResponse<CarDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CarCreateDto carDto)
        {
            var car = _mapper.Map<Car>(carDto);
            
            // Set ค่า Default
            car.CreatedAt = DateTime.UtcNow;
            car.IsUsed = true; // ตามงานวิจัยคือรถมือสอง
            car.IsApproved = false; // รอแอดมินอนุมัติ
            car.IsDeleted = false;
            car.CarStatus = SD.Status_Available; // ค่า Default รอขาย

            // 1. จัดการรูปภาพใหม่ (ถ้ามี)
            var uploadedImageUrls = new List<string>();
            if (carDto.NewImages != null && carDto.NewImages.Any())
            {
                foreach (var file in carDto.NewImages)
                {
                    var uploadedPath = await _fileUpload.UploadFile(file, SD.ImgProductPath);
                    if (!string.IsNullOrEmpty(uploadedPath))
                    {
                        uploadedImageUrls.Add(uploadedPath);
                    }
                }
            }

            // นำ List ของ URL ไปยัดใส่ Property FileList (ที่จะแปลงเป็น JSON ลง DB)
            car.CarImages = uploadedImageUrls;

            var created = await _carRepo.AddAsync(car);
            var result = _mapper.Map<CarDto>(created);

            return Ok(ApiResponse<CarDto>.Success(result, "เพิ่มรถเข้าสู่ระบบเรียบร้อย รอการอนุมัติ"));
        }

        [HttpPut("update/{carId}")]
        public async Task<IActionResult> Update([FromForm] CarUpdateDto carDto, int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณต้องการแก้ไข"));

            // 1. ดึงรายการรูปเก่าที่มีอยู่เดิมใน Database
            var currentImages = car.CarImages ?? new List<string>();
            var imagesToKeep = carDto.KeepImages ?? new List<string>();
            var finalImages = new List<string>();

            // 2. ลบไฟล์จริงออกจากเซิร์ฟเวอร์ (สำหรับรูปที่ User ไม่ได้ส่งมาใน KeepImages)
            foreach (var imgUrl in currentImages)
            {
                if (imagesToKeep.Contains(imgUrl))
                {
                    // รูปนี้ยังอยากเก็บไว้
                    finalImages.Add(imgUrl);
                }
                else
                {
                    // รูปนี้โดนสั่งลบ ให้ลบไฟล์ออกจาก Server จริงๆ
                    _fileUpload.DeleteFile(imgUrl);
                }
            }

            // 3. อัปโหลดรูปใหม่ (ถ้ามีการแนบมาเพิ่ม)
            if (carDto.NewImages != null && carDto.NewImages.Any())
            {
                foreach (var file in carDto.NewImages)
                {
                    var uploadedPath = await _fileUpload.UploadFile(file, SD.ImgProductPath);
                    if (!string.IsNullOrEmpty(uploadedPath))
                    {
                        finalImages.Add(uploadedPath);
                    }
                }
            }

            // 4. Map ข้อมูลใหม่จาก DTO ทับลงไปในตัวแปล car (AutoMapper จะข้ามฟิลด์ที่ค่าเป็น null)
            _mapper.Map(carDto, car);

            // 5. อัปเดตข้อมูลรูปภาพชุดสุดท้าย และ Timestamp
            car.CarImages = finalImages;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepo.UpdateAsync(car);
            
            var result = _mapper.Map<CarDto>(car);
            return Ok(ApiResponse<CarDto>.Success(result, "อัปเดตข้อมูลรถเรียบร้อย"));
        }

        [HttpPut("delete/{carId}")]
        public async Task<IActionResult> Delete(int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณต้องการลบ"));

            // เปลี่ยนจาก Hard Delete เป็น Soft Delete 
            // ไม่ควรลบไฟล์ภาพจริงๆ ทิ้ง เผื่อต้องการกู้ข้อมูลคืน
            car.IsDeleted = true;
            car.UpdatedAt = DateTime.UtcNow;

            await _carRepo.UpdateAsync(car);
            return Ok(ApiResponse<string>.Success("ลบรถเรียบร้อยแล้ว (Soft Delete)"));
        }

        [HttpPut("ApproveCar")]
        public async Task<IActionResult> ApproveCar([FromBody] ApprovalCreateDto dto)
        {
            var car = await _carRepo.GetByIdAsync(dto.CarId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่ต้องการอนุมัติ"));

            car.IsApproved = dto.IsApproved;
            car.ApprovalRemark = dto.Remark;
            
            if (dto.IsApproved)
            {
                car.ApprovedAt = DateTime.UtcNow;
                car.CarStatus = SD.Status_Available;
            }
            else
            {
                car.CarStatus = SD.Status_Sold; // หรือสถานะอื่นตามต้องการเมื่อไม่อนุมัติ
            }

            car.UpdatedAt = DateTime.UtcNow;
            await _carRepo.UpdateAsync(car);

            return Ok(ApiResponse<string>.Success("อัปเดตสถานะการอนุมัติเรียบร้อย"));
        }
    }
}