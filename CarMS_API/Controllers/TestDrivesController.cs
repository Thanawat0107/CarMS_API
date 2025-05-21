using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto;
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
    public class TestDrivesController : ControllerBase
    {
        private readonly IRepository<TestDrive> _TestDriveRepo;
        private readonly ISearchableRepository<TestDrive, TestDriveSearchParams> _searchRepo;
        private readonly IMapper _mapper;
        public TestDrivesController(IRepository<TestDrive> TestDriveRepo,
            ISearchableRepository<TestDrive, TestDriveSearchParams> searchRepo,
            IMapper mapper)
        {
            _TestDriveRepo = TestDriveRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TestDriveSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (TestDrives, totalCount) = await _TestDriveRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<TestDriveDto>>(TestDrives);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<TestDriveDto>>.Success(result, "โหลดรายการทดลองขับสำเร็จ", pagination));
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

            testDrive.StatusTestDrive = StatusTestDrive.Pending;
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
