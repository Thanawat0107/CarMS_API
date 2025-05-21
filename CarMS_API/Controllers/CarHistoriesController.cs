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
    public class CarHistoriesController : ControllerBase
    {
        private readonly IRepository<CarHistory> _carHistoryRepo;
        private readonly ISearchableRepository<CarHistory, CarHistorySearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public CarHistoriesController(IRepository<CarHistory> carHistoryRepo, 
            ISearchableRepository<CarHistory, 
            CarHistorySearchParams> searchRepo,
            IMapper mapper)
        {
            _carHistoryRepo = carHistoryRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarHistorySearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (histories, totalCount) = await _carHistoryRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<CarHistoryDto>>(histories);
            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<CarHistoryDto>>.Success(result, "โหลดประวัติรถเรียบร้อย", pagination));
        }

        [HttpGet("{carHistoryId}")]
        public async Task<IActionResult> GetById(int carHistoryId)
        {
            var carHistory = await _carHistoryRepo.GetByIdAsync(carHistoryId, q => q.Include(c => c.Car));
            if (carHistory == null) return NotFound(ApiResponse<string>.Fail("ไม่พบประวัติรถ"));

            var result = _mapper.Map<CarHistoryDto>(carHistory);

            return Ok(ApiResponse<CarHistoryDto>.Success(result, "โหลดประวัติรถเรียบร้อย"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CarHistoryCreateDto carHistoryDto)
        {
            var carHistory = _mapper.Map<CarHistory>(carHistoryDto);
            carHistory.CreatedAt = DateTime.UtcNow;
            carHistory.IsApproved = false;
            carHistory.IsUsed = true;
            carHistory.IsDeleted = false;

            var created = await _carHistoryRepo.AddAsync(carHistory);
            var result = _mapper.Map<CarHistoryCreateDto>(created);

            return Ok(ApiResponse<CarHistoryCreateDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{carHistoryId}")]
        public async Task<IActionResult> Update(CarHistoryCreateDto carHistoryCreateDto)
        {
            var carHistory = await _carHistoryRepo.GetByIdAsync(carHistoryCreateDto.Id);
            if (carHistory == null) return NotFound(ApiResponse<string>.Fail("ไม่พบประวัติรถที่ต้องการแก้ไข"));
            
            _mapper.Map(carHistoryCreateDto, carHistory);
            carHistory.UpdatedAt = DateTime.UtcNow;
            await _carHistoryRepo.UpdateAsync(carHistory);

            var result = _mapper.Map<CarHistoryCreateDto>(carHistory);
            return Ok(ApiResponse<CarHistoryCreateDto>.Success(result, "แก้ไขประวัติรถสำเร็จ"));
        }

        [HttpDelete("{carHistoryId}")]
        public async Task<IActionResult> Delete(int carHistoryId) 
        {
            var deleted = await _carHistoryRepo.DeleteAsync(carHistoryId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail("ไม่พบประวัติรถที่ต้องการลบ"));

            return Ok(ApiResponse<string>.Success("ลบประวัติรถสำเร็จ"));
        }
    }
}
