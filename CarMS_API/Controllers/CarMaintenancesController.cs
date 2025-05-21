using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarMaintenancesController : ControllerBase
    {
        private readonly IRepository<CarMaintenance> _carMaintenanceRepo;
        private readonly ISearchableRepository<CarMaintenance, CarMaintenanceSearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public CarMaintenancesController(
            IRepository<CarMaintenance> carMaintenanceRepo,
            ISearchableRepository<CarMaintenance, CarMaintenanceSearchParams> searchRepo, 
            IMapper mapper
        )
        {
            _carMaintenanceRepo = carMaintenanceRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarMaintenanceSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (maintenances, totalCount) = await _carMaintenanceRepo
                .GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<CarMaintenanceDto>>(maintenances);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse
                <IEnumerable<CarMaintenanceDto>>
                .Success(result, 
                "โหลดรายการบำรุงรักษารถเรียบร้อย",
                pagination));
        }

        [HttpGet("{carMaintenanceId}")]
        public async Task<IActionResult> GetById(int carMaintenanceId)
        {
            var carMaintenance = await _carMaintenanceRepo
                .GetByIdAsync(carMaintenanceId,
                q => q.Include(c => c.CarHistory).ThenInclude(c => c.Car));
            if (carMaintenance == null) 
                return NotFound(ApiResponse<string>
                    .Fail("ไม่พบการบำรุงรักษารถ"));

            var result = _mapper.Map<CarMaintenanceDto>(carMaintenance);

            return Ok(
                ApiResponse<CarMaintenanceDto>
                .Success(result,
                "โหลดการบำรุงรักษารถเรียบร้อย"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarMaintenanceCreateDto carMaintenanceDto)
        {
            var carMaintenance = _mapper.Map<CarMaintenance>(carMaintenanceDto);
            carMaintenance.ServiceDate = DateTime.UtcNow;
            carMaintenance.IsUsed = true;
            carMaintenance.IsDeleted = false;

            var created = await _carMaintenanceRepo
                .AddAsync(carMaintenance);
            var result = _mapper.Map<CarMaintenanceCreateDto>(created);

            return Ok(
                ApiResponse<CarMaintenanceCreateDto>
                .Success(result,
                "สำเร็จ"));
        }

        [HttpPut("{carMaintenanceId}")]
        public async Task<IActionResult> Update(CarMaintenanceCreateDto carMaintenanceCreateDto)
        {
            var carMaintenance = await _carMaintenanceRepo
                .GetByIdAsync(carMaintenanceCreateDto.Id);
            if (carMaintenance == null) 
                return NotFound(ApiResponse<string>
                    .Fail("ไม่พบการบำรุงรักษารถที่ต้องการแก้ไข"));

            _mapper.Map(carMaintenanceCreateDto, carMaintenance);
            await _carMaintenanceRepo.UpdateAsync(carMaintenance);

            var result = _mapper
                .Map<CarMaintenanceCreateDto>(carMaintenance);
            return Ok(
                ApiResponse<CarMaintenanceCreateDto>
                .Success(result, "แก้ไขการบำรุงรักษารถสำเร็จ"));
        }

        [HttpDelete("{carMaintenanceId}")]
        public async Task<IActionResult> Delete(int carMaintenanceId)
        {
            var deleted = await _carMaintenanceRepo
                .DeleteAsync(carMaintenanceId);
            if (deleted == null) 
                return NotFound(ApiResponse<string>
                    .Fail("ไม่พบการบำรุงรักษารถที่ต้องการลบ"));

            return Ok(
                ApiResponse<string>
                .Success("ลบการบำรุงรักษารถสำเร็จ"));
        }

    }
}