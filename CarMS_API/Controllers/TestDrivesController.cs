using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDrivesController : ControllerBase
    {
        private readonly IRepository<TestDrive> _TestDriveRepo;
        private readonly IRepository<Car> _carRepo; // 🌟 แนะนำให้เพิ่ม CarRepo เพื่อเช็คว่ารถมีอยู่จริง
        private readonly IMapper _mapper;

        public TestDrivesController(
            IRepository<TestDrive> TestDriveRepo,
            IRepository<Car> carRepo, // Inject เพิ่มเข้ามา
            IMapper mapper)
        {
            _TestDriveRepo = TestDriveRepo;
            _carRepo = carRepo;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (testDrives, totalCount) = await _TestDriveRepo.GetAllAsync(
                include: query => query
                    .Include(q => q.Car),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<TestDriveDto>>(testDrives);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<TestDriveDto>>.Success(result, "โหลดรายการทดลองขับสำเร็จ", meta));
        }

        [HttpGet("getbyid/{testDriveId}")]
        public async Task<IActionResult> GetById(int testDriveId)
        {
            var TestDrive = await _TestDriveRepo.GetByIdAsync(testDriveId, 
                q => q.Include(c => c.Car));
            
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการทดลองขับที่คุณค้นหา"));
            var result = _mapper.Map<TestDriveDto>(TestDrive);

            return Ok(ApiResponse<TestDriveDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TestDriveCreateDto TestDriveDto)
        {
            // 🌟 เช็คว่ารถมีอยู่จริงและพร้อมให้ลองขับ
            var car = await _carRepo.GetByIdAsync(TestDriveDto.CarId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบข้อมูลรถยนต์"));

            // 🌟 เช็คไม่ให้จองวันย้อนหลัง
            if (TestDriveDto.AppointmentDate < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถนัดหมายทดลองขับในอดีตได้"));

            var testDrive = _mapper.Map<TestDrive>(TestDriveDto);

            testDrive.CreatedAt = DateTime.UtcNow; // 🌟 เพิ่ม CreatedAt
            testDrive.StatusTestDrive = SD.TestDrive_Pending;

            await _TestDriveRepo.AddAsync(testDrive);
            var result = _mapper.Map<TestDriveCreateDto>(testDrive);

            return Ok(ApiResponse<TestDriveCreateDto>.Success(result, "ขอนัดหมายทดลองขับสำเร็จ รอผู้ขายยืนยัน"));
        }

        [HttpPut("update/{testDriveId}")]
        public async Task<IActionResult> Update(int testDriveId, [FromBody] TestDriveCreateDto TestDriveDto)
        {
            // 🌟 แก้บัค: ใช้ testDriveId จาก URL ในการค้นหา
            var TestDrive = await _TestDriveRepo.GetByIdAsync(testDriveId);
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการทดลองขับที่คุณต้องการแก้ไข"));

            _mapper.Map(TestDriveDto, TestDrive);
            
            await _TestDriveRepo.UpdateAsync(TestDrive);

            var result = _mapper.Map<TestDriveCreateDto>(TestDrive);
            return Ok(ApiResponse<TestDriveCreateDto>.Success(result, "อัปเดตสถานะนัดหมายเรียบร้อย"));
        }

        [HttpDelete("delete/{testDriveId}")]
        public async Task<IActionResult> Delete(int testDriveId)
        {
            var deleted = await _TestDriveRepo.DeleteAsync(testDriveId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบรายการทดลองขับ ID: {testDriveId}"));

            return Ok(ApiResponse<string>.Success("ลบรายการทดลองขับเรียบร้อยแล้ว"));
        }
    }
}