using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
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
        public async Task<IActionResult> GetAll(int? carId, int pageNumber = 1, int pageSize = 10)
        {
            // เพิ่มการกรองข้อมูล: ถ้ามีการส่ง carId มา ให้แสดงเฉพาะของรถคันนั้น และไม่แสดงรายการที่ถูกลบ (IsDeleted = false)
            var (maintenances, totalCount) = await _carMaintenanceRepo.GetAllAsync(
                filter: q => (!carId.HasValue || q.CarId == carId.Value) && !q.IsDeleted,
                include: query => query.Include(q => q.Car), // ดึงข้อมูลรถมาด้วย
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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CarMaintenanceCreateDto carMaintenanceDto)
        {
            var carMaintenance = _mapper.Map<CarMaintenance>(carMaintenanceDto);
            
            // ป้องกันกรณีไม่ได้ส่งวันที่มา ให้ใช้วันที่ปัจจุบัน
            if (carMaintenance.ServiceDate == default)
            {
                carMaintenance.ServiceDate = DateTime.UtcNow;
            }
            
            carMaintenance.IsUsed = true;
            carMaintenance.IsDeleted = false;

            var created = await _carMaintenanceRepo.AddAsync(carMaintenance);
            var result = _mapper.Map<CarMaintenanceDto>(created);

            return Ok(ApiResponse<CarMaintenanceDto>.Success(result, "เพิ่มประวัติการซ่อมบำรุงสำเร็จ"));
        }

        [HttpPut("update/{carMaintenanceId}")]
        public async Task<IActionResult> Update(int carMaintenanceId, [FromBody] CarMaintenanceUpdateDto carMaintenanceUpdateDto)
        {
            // ค้นหาประวัติที่ต้องการแก้ไขด้วย URL Parameter
            var carMaintenance = await _carMaintenanceRepo.GetByIdAsync(carMaintenanceId);
            
            if (carMaintenance == null) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบการบำรุงรักษารถที่ต้องการแก้ไข"));

            _mapper.Map(carMaintenanceUpdateDto, carMaintenance);
            
            await _carMaintenanceRepo.UpdateAsync(carMaintenance);

            var result = _mapper.Map<CarMaintenanceDto>(carMaintenance);
            return Ok(ApiResponse<CarMaintenanceDto>.Success(result, "แก้ไขการบำรุงรักษารถสำเร็จ"));
        }

        [HttpPut("delete/{carMaintenanceId}")]
        public async Task<IActionResult> Delete(int carMaintenanceId)
        {
            var carMaintenance = await _carMaintenanceRepo.GetByIdAsync(carMaintenanceId);
            
            if (carMaintenance == null) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบการบำรุงรักษารถที่ต้องการลบ"));

            // เปลี่ยนมาใช้ Soft Delete เพื่อเก็บข้อมูลไว้ดูย้อนหลัง
            carMaintenance.IsDeleted = true;
            await _carMaintenanceRepo.UpdateAsync(carMaintenance);

            return Ok(ApiResponse<string>.Success("ลบการบำรุงรักษารถสำเร็จ (Soft Delete)"));
        }
    }
}