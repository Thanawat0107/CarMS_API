using AutoMapper;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarMS_API.Models.Dto.CreateDto;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalsController : ControllerBase
    {
        private readonly IRepository<Approval> _ApprovalRepo;
        private readonly IRepository<CarHistory> _CarHistoryRepo;
        private readonly ISearchableRepository<Approval, ApprovalSearchParams> _searchRepo;
        private readonly IMapper _mapper;
        public ApprovalsController(IRepository<Approval> ApprovalRepo,
            IRepository<CarHistory> carHistoryRepo,
            ISearchableRepository<Approval, ApprovalSearchParams> searchRepo,
            IMapper mapper)
        {
            _ApprovalRepo = ApprovalRepo;
            _CarHistoryRepo = carHistoryRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ApprovalSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (Approvals, totalCount) = await _ApprovalRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<ApprovalDto>>(Approvals);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<ApprovalDto>>.Success(result, "โหลดรายการการอนุมัติสำเร็จ", pagination));
        }

        [HttpGet("{ApprovalId}")]
        public async Task<IActionResult> GetById(int ApprovalId)
        {
            var Approval = await _ApprovalRepo.GetByIdAsync(ApprovalId, q => q.Include(c => c.CarHistory));
            if (Approval == null) return NotFound(ApiResponse<string>.Fail("ไม่พบการอนุมัติที่คุณค้นหา"));
            var result = _mapper.Map<ApprovalDto>(Approval);

            return Ok(ApiResponse<ApprovalDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApprovalCreateDto ApprovalDto)
        {
            var carHistory = await _CarHistoryRepo.GetByIdAsync(
                ApprovalDto.CarHistoryId,
                include: query => query.Include(ch => ch.Car)
            );

            if (carHistory == null || carHistory.Car == null)
                return NotFound(ApiResponse<string>.Fail("ไม่พบ CarHistory หรือ Car"));

            var approval = _mapper.Map<Approval>(ApprovalDto);
            approval.ApprovedAt = DateTime.UtcNow;
            if (approval.CarHistory?.Car != null)
            {
                approval.CarHistory.Car.IsApproved = true;
            }

            await _ApprovalRepo.AddAsync(approval);
            var result = _mapper.Map<ApprovalCreateDto>(approval);

            return Ok(ApiResponse<ApprovalCreateDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{ApprovalId}")]
        public async Task<IActionResult> Update(ApprovalCreateDto ApprovalDto)
        {
            var approval = await _ApprovalRepo.GetByIdAsync(
                ApprovalDto.Id,
                include: query => query
                .Include(a => a.CarHistory)
                .ThenInclude(ch => ch.Car)
               );

            if (approval == null) return NotFound(ApiResponse<string>.Fail("ไม่พบการอนุมัติที่คุณต้องการแก้ไข"));

            _mapper.Map(ApprovalDto, approval);

            if (approval.CarHistory?.Car != null)
            {
                approval.CarHistory.Car.IsApproved = true;
            }

            await _ApprovalRepo.UpdateAsync(approval);

            var result = _mapper.Map<ApprovalCreateDto>(approval);
            return Ok(ApiResponse<ApprovalCreateDto>.Success(result, "อัปเดตการอนุมัติเรียบร้อย"));
        }

        [HttpDelete("{ApprovalId}")]
        public async Task<IActionResult> Delete(int ApprovalId)
        {
            var deleted = await _ApprovalRepo.DeleteAsync(ApprovalId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบการอนุมัติ ID: {ApprovalId}"));

            return Ok(ApiResponse<string>.Success("ลบการอนุมัติเรียบร้อยแล้ว"));
        }
    }
}
