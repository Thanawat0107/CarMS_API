using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {
        private readonly IRepository<Seller> _sellerRepo;
        private readonly ISearchableRepository<Seller, SellerSearchParams> _searchRepo;
        private readonly IMapper _mapper;

        public SellersController(IRepository<Seller> sellerRepo,
            ISearchableRepository<Seller, SellerSearchParams> searchRepo,
            IMapper mapper)
        {
            _sellerRepo = sellerRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SellerSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (sellers, totalCount) = await _sellerRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<SellerDto>>(sellers);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<SellerDto>>.Success(result, "โหลดรายการผู้ขายสำเร็จ", pagination));
        }

        [HttpGet("{sellerId}")]
        public async Task<IActionResult> GetById(int sellerId)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId);
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณค้นหา"));
            var result = _mapper.Map<SellerDto>(seller);

            return Ok(ApiResponse<SellerDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost]
        public async Task<IActionResult> Create(SellerDto sellerDto)
        {
            var seller = _mapper.Map<Seller>(sellerDto);
            seller.IsVerified = true;

            var created = await _sellerRepo.AddAsync(seller);
            var result = _mapper.Map<SellerDto>(seller);

            return Ok(ApiResponse<SellerDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{sellerId}")]
        public async Task<IActionResult> Update(SellerDto sellerDto)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerDto.Id);
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณต้องการแก้ไข"));

            _mapper.Map(sellerDto, seller);
            await _sellerRepo.UpdateAsync(seller);

            var result = _mapper.Map<SellerDto>(seller);
            return Ok(ApiResponse<SellerDto>.Success(result, "อัปเดตผู้ขายเรียบร้อย"));
        }

        [HttpDelete("{sellerId}")]
        public async Task<IActionResult> Delete(int sellerId)
        {
            var deleted = await _sellerRepo.DeleteAsync(sellerId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบผู้ขาย ID: {sellerId}"));

            return Ok(ApiResponse<string>.Success("ลบผู้ขายเรียบร้อยแล้ว"));
        }
    }
}
