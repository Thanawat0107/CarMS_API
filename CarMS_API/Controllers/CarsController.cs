using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly IRepository<Car> _carRepo;
        private readonly ISearchableRepository<Car, CarSearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public CarsController(IRepository<Car> carRepo,
            ISearchableRepository<Car, CarSearchParams> searchRepo,
            IMapper mapper)
        {
            _carRepo = carRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (cars, totalCount) = await _carRepo.GetAllAsync(
                filter,
                orderBy,
                include: _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<CarDto>>(cars);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<CarDto>>.Success(result, "โหลดรายการรถเรียบร้อย", pagination));
        }

        [HttpGet("{carId}")]
        public async Task<IActionResult> GetById(int carId)
        {
            var car = await _carRepo.GetByIdAsync(carId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณค้นหา"));
            var result = _mapper.Map<CarDto>(car);

            return Ok(ApiResponse<CarDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarDto carDto)
        {
            var car = _mapper.Map<Car>(carDto);
            car.CreatedAt = DateTime.UtcNow;
            car.IsUsed = false;
            car.IsDeleted = false;

            var created = await _carRepo.AddAsync(car);
            var result = _mapper.Map<CarDto>(created);

            return Ok(ApiResponse<CarDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{carId}")]
        public async Task<IActionResult> Update(CarDto carDto)
        {
            var car = await _carRepo.GetByIdAsync(carDto.Id);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถที่คุณต้องการแก้ไข"));

            _mapper.Map(carDto, car);
            car.UpdatedAt = DateTime.UtcNow;
            await _carRepo.UpdateAsync(car);

            var result = _mapper.Map<CarDto>(car);
            return Ok(ApiResponse<CarDto>.Success(result, "อัปเดตรถเรียบร้อย"));
        }

        [HttpDelete("{carId}")]
        public async Task<IActionResult> Delete(int carId)
        {
            var deleted = await _carRepo.DeleteAsync(carId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบรถ ID: {carId}"));

            return Ok(ApiResponse<string>.Success("ลบรถเรียบร้อยแล้ว"));
        }
    }
}
