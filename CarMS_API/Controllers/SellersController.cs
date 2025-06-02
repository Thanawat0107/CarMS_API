using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {
        private readonly IRepository<Seller> _sellerRepo;
        private readonly ISearchableRepository<Seller, SellerSearchParams> _searchRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellersController(IRepository<Seller> sellerRepo,
            ISearchableRepository<Seller, SellerSearchParams> searchRepo,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _sellerRepo = sellerRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("getall")]
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

        [HttpGet("getbyid/{sellerId}")]
        public async Task<IActionResult> GetById(int sellerId)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId, q => q.Include(q => q.User));
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณค้นหา"));
            var result = _mapper.Map<SellerDto>(seller);

            return Ok(ApiResponse<SellerDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SellerCreateDto sellerDto)
        {
            var existingSeller = await _sellerRepo.FirstOrDefaultAsync(s => s.UserId == sellerDto.UserId);
            if (existingSeller != null)
            {
                return BadRequest(ApiResponse<string>.Fail("ผู้ใช้นี้ได้ลงทะเบียนเป็นผู้ขายแล้ว"));
            }

            var user = await _userManager.FindByIdAsync(sellerDto.UserId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ใช้ที่เชื่อมโยงกับ Seller"));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            await _userManager.AddToRoleAsync(user, SD.Role_Seller);

            var seller = _mapper.Map<Seller>(sellerDto);
            seller.IsVerified = false;

            var created = await _sellerRepo.AddAsync(seller);
            var result = _mapper.Map<SellerCreateDto>(created);

            return Ok(ApiResponse<SellerCreateDto>.Success(result, "สร้างผู้ขายและอัปเดต Role เรียบร้อยแล้ว"));
        }

        [HttpPut("update/{sellerId}")]
        public async Task<IActionResult> Update([FromBody] SellerCreateDto sellerDto, int sellerId)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId);
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณต้องการแก้ไข"));

            _mapper.Map(sellerDto, seller);
            await _sellerRepo.UpdateAsync(seller);

            var result = _mapper.Map<SellerDto>(seller);
            return Ok(ApiResponse<SellerDto>.Success(result, "อัปเดตผู้ขายเรียบร้อย"));
        }

        [HttpDelete("delete/{sellerId}")]
        public async Task<IActionResult> Delete(int sellerId)
        {
            var deleted = await _sellerRepo.DeleteAsync(sellerId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบผู้ขาย ID: {sellerId}"));

            return Ok(ApiResponse<string>.Success("ลบผู้ขายเรียบร้อยแล้ว"));
        }
    }
}