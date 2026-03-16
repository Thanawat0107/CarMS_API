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
        private readonly IMapper _mapper;
        public TestDrivesController(IRepository<TestDrive> TestDriveRepo,
            IMapper mapper)
        {
            _TestDriveRepo = TestDriveRepo;
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

        [HttpGet("{TestDriveId}")]
        public async Task<IActionResult> GetById(int TestDriveId)
        {
            var TestDrive = await _TestDriveRepo.GetByIdAsync(TestDriveId, q => q.Include(c => c.Car));
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบทดลองขับที่คุณค้นหา"));
            var result = _mapper.Map<TestDriveDto>(TestDrive);

            return Ok(ApiResponse<TestDriveDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(TestDriveCreateDto TestDriveDto)
        {
            var testDrive = _mapper.Map<TestDrive>(TestDriveDto);

            testDrive.StatusTestDrive = SD.TestDrive_Pending;
            await _TestDriveRepo.AddAsync(testDrive);
            var result = _mapper.Map<TestDriveCreateDto>(testDrive);

            return Ok(ApiResponse<TestDriveCreateDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{TestDriveId}")]
        public async Task<IActionResult> Update(TestDriveCreateDto TestDriveDto)
        {
            var TestDrive = await _TestDriveRepo.GetByIdAsync(TestDriveDto.Id);
            if (TestDrive == null) return NotFound(ApiResponse<string>.Fail("ไม่พบทดลองขับที่คุณต้องการแก้ไข"));

            _mapper.Map(TestDriveDto, TestDrive);
            await _TestDriveRepo.UpdateAsync(TestDrive);

            var result = _mapper.Map<TestDriveCreateDto>(TestDrive);
            return Ok(ApiResponse<TestDriveCreateDto>.Success(result, "อัปเดตทดลองขับเรียบร้อย"));
        }

        [HttpDelete("{TestDriveId}")]
        public async Task<IActionResult> Delete(int TestDriveId)
        {
            var deleted = await _TestDriveRepo.DeleteAsync(TestDriveId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบทดลองขับ ID: {TestDriveId}"));

            return Ok(ApiResponse<string>.Success("ลบทดลองขับเรียบร้อยแล้ว"));
        }
    }
}
