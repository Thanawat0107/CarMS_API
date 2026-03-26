using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDrivesController : ControllerBase
    {
        private readonly IRepository<TestDrive> _TestDriveRepo;
        private readonly IRepository<Car> _carRepo;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TestDrivesController(
            IRepository<TestDrive> TestDriveRepo,
            IRepository<Car> carRepo,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _TestDriveRepo = TestDriveRepo;
            _carRepo = carRepo;
            _mapper = mapper;
            _hubContext = hubContext;

        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (testDrives, totalCount) = await _TestDriveRepo.GetAllAsync(
                include: query => query.Include(q => q.Car).Include(q => q.User),
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
                q => q.Include(c => c.Car).Include(c => c.User));
            
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการทดลองขับที่คุณค้นหา"));
            var result = _mapper.Map<TestDriveDto>(TestDrive);

            return Ok(ApiResponse<TestDriveDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TestDriveCreateDto TestDriveDto)
        {
            // Include ข้อมูลผู้ขาย (Seller) และยี่ห้อ (Brand) มาด้วยเพื่อใช้แจ้งเตือน
            var car = await _carRepo.GetByIdAsync(TestDriveDto.CarId, q => q.Include(c => c.Seller).Include(c => c.Brand));
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบข้อมูลรถยนต์"));

            if (TestDriveDto.AppointmentDate < DateTime.UtcNow)
                return BadRequest(ApiResponse<string>.Fail("ไม่สามารถนัดหมายทดลองขับในอดีตได้"));

            var testDrive = _mapper.Map<TestDrive>(TestDriveDto);
            testDrive.CreatedAt = DateTime.UtcNow;
            testDrive.StatusTestDrive = SD.TestDrive_Pending;

            var created = await _TestDriveRepo.AddAsync(testDrive);
            var result = _mapper.Map<TestDriveCreateDto>(testDrive);

            if (car.Seller != null && !string.IsNullOrEmpty(car.Seller.UserId))
            {
                var notificationMessage = new 
                {
                    Title = "ลูกค้านัดหมายทดลองขับ! 🚗",
                    Message = $"รถ {car.Brand?.Name} {car.Model} มีลูกค้านัดหมายวันที่ {testDrive.AppointmentDate.ToLocalTime():dd/MM/yyyy HH:mm} โปรดตรวจสอบและยืนยันคิว",
                    TestDriveId = created.Id
                };
                await _hubContext.Clients.Group(car.Seller.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

            return Ok(ApiResponse<TestDriveCreateDto>.Success(result, "ขอนัดหมายทดลองขับสำเร็จ รอผู้ขายยืนยัน"));
        }

        [HttpPut("update/{testDriveId}")]
        public async Task<IActionResult> Update(int testDriveId, [FromBody] TestDriveCreateDto TestDriveDto)
        {
            // Include รถยนต์มาด้วยเพื่อเอาชื่อรุ่นไปโชว์ในแจ้งเตือน
            var TestDrive = await _TestDriveRepo.GetByIdAsync(testDriveId, q => q.Include(t => t.Car));
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการทดลองขับที่คุณต้องการแก้ไข"));

            // อัปเดตข้อมูล
            _mapper.Map(TestDriveDto, TestDrive);
            await _TestDriveRepo.UpdateAsync(TestDrive);

            var result = _mapper.Map<TestDriveCreateDto>(TestDrive);

            // ⚡ สั่ง SignalR: ยิงแจ้งเตือนไปหา "ลูกค้า (UserId ของคนขอทดลองขับ)"
            if (!string.IsNullOrEmpty(TestDrive.UserId))
            {
                var statusText = TestDrive.StatusTestDrive == SD.TestDrive_Confirmed ? "ได้รับการยืนยันแล้ว" : 
                                 TestDrive.StatusTestDrive == SD.TestDrive_Cancel ? "ถูกยกเลิก" : "ถูกอัปเดตสถานะ";
                
                var notificationMessage = new 
                {
                    Title = "อัปเดตสถานะคิวทดลองขับ 🔔",
                    Message = $"นัดหมายทดลองขับรถ {TestDrive.Car?.Model} ของคุณ {statusText}",
                    TestDriveId = TestDrive.Id
                };
                await _hubContext.Clients.Group(TestDrive.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

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