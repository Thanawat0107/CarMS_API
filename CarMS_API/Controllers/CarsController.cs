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
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (cars, totalCount) = await _carRepo.GetAllAsync(
                include: query => query
                    .Include(q => q.Seller)
                    .Include(q => q.Brand),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<CarDto>>(cars);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<CarDto>>.Success(result, "โหลดรายการรถเรียบร้อย", meta));
        }

        [HttpGet("getbyid/{carId}")]
        public async Task<IActionResult> GetById(int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId, 
                q=>q.Include(q=>q.Seller)
                .Include(q=>q.Brand));

            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณค้นหา"));
            var result = _mapper.Map<CarDto>(car);

            return Ok(ApiResponse<CarDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] CarCreateDto carDto)
        {
            var car = _mapper.Map<Car>(carDto);
            car.CreatedAt = DateTime.UtcNow;
            car.IsUsed = false;
            car.IsApproved = false;
            car.IsDeleted = false;

            // อัปโหลดภาพ
            if (carDto.ImageFile != null)
            {
                car.CarImages = await _fileUpload.UploadFile(carDto.ImageFile, SD.ImgProductPath);
            }

            var created = await _carRepo.AddAsync(car);
            var result = _mapper.Map<CarCreateDto>(created);

            return Ok(ApiResponse<CarCreateDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("update/{carId}")]
        public async Task<IActionResult> Update([FromForm] CarUpdateDto carDto, int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณต้องการแก้ไข"));
            _mapper.Map(carDto, car);
            car.UpdatedAt = DateTime.UtcNow;

            // อัปเดตภาพ
            if (carDto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(car.CarImages))
                {
                    _fileUpload.DeleteFile(car.CarImages);
                }
                car.CarImages = await _fileUpload.UploadFile(carDto.ImageFile, SD.ImgProductPath);
            }

            await _carRepo.UpdateAsync(car);
            var result = _mapper.Map<CarCreateDto>(car);
            return Ok(ApiResponse<CarCreateDto>.Success(result, "อัปเดตรถเรียบร้อย"));
        }

        [HttpPut("delete/{carId}")]
        public async Task<IActionResult> Delete(int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณต้องการลบ"));

            // ลบไฟล์ภาพก่อนลบสินค้า
            if (!string.IsNullOrEmpty(car.CarImages))
            {
                _fileUpload.DeleteFile(car.CarImages);
            }

            car.IsDeleted = true;
            await _carRepo.UpdateAsync(car);
            return Ok(ApiResponse<string>.Success("ลบรถเรียบร้อยแล้ว"));
        }
    }
}
