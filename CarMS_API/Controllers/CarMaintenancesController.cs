using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
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
        private readonly IMapper _mapper;

        public CarMaintenancesController(
            IRepository<CarMaintenance> carMaintenanceRepo,
            IMapper mapper
        )
        {
            _carMaintenanceRepo = carMaintenanceRepo;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (maintenances, totalCount) = await _carMaintenanceRepo.GetAllAsync(
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<CarMaintenanceDto>>(maintenances);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<CarMaintenanceDto>>.Success(result, "โหลดรายการบำรุงรักษารถเรียบร้อย", meta));
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