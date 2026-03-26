using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
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
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellersController(IRepository<Seller> sellerRepo,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _sellerRepo = sellerRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(bool? isVerified, int pageNumber = 1, int pageSize = 10) // 🌟 เพิ่ม bool? isVerified
        {
            var (sellers, totalCount) = await _sellerRepo.GetAllAsync(
                filter: q => !isVerified.HasValue || q.IsVerified == isVerified.Value,
                include: query => query.Include(q => q.User), 
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<SellerDto>>(sellers);
            var meta = new PaginationMeta { TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };

            return Ok(ApiResponse<IEnumerable<SellerDto>>.Success(result, "โหลดรายการผู้ขายสำเร็จ", meta));
        }

        [HttpGet("getbyid/{sellerId}")]
        public async Task<IActionResult> GetById(int sellerId)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId, q => q.Include(q => q.User));
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณค้นหา"));
            var result = _mapper.Map<SellerDto>(seller);

            return Ok(ApiResponse<SellerDto>.Success(result, "สำเร็จ"));
        }

        // สำหรับ User ทั่วไปที่ต้องการสมัครเป็นคนขาย
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SellerCreateDto sellerDto)
        {
            var existingSeller = await _sellerRepo.FirstOrDefaultAsync(s => s.UserId == sellerDto.UserId);
            if (existingSeller != null)
            {
                return BadRequest(ApiResponse<string>.Fail("ผู้ใช้นี้ได้ลงทะเบียนข้อมูลผู้ขายไว้แล้ว"));
            }

            var user = await _userManager.FindByIdAsync(sellerDto.UserId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ใช้ในระบบ"));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }

            // เปลี่ยน Role เป็น Seller
            await _userManager.AddToRoleAsync(user, SD.Role_Seller);

            var seller = _mapper.Map<Seller>(sellerDto);
            seller.IsVerified = false; // รอแอดมินยืนยันตัวตน

            var created = await _sellerRepo.AddAsync(seller);
            var result = _mapper.Map<SellerDto>(created); // 🌟 เปลี่ยนเป็นส่ง SellerDto กลับไปแทน

            return Ok(ApiResponse<SellerDto>.Success(result, "ส่งข้อมูลลงทะเบียนผู้ขายเรียบร้อย รอการตรวจสอบ"));
        }

        // 🌟 เพิ่มใหม่: เมธอดสำหรับ Admin กดยืนยันตัวตนให้คนขาย
        [HttpPut("verify/{sellerId}")]
        public async Task<IActionResult> VerifySeller(int sellerId, [FromBody] bool isVerified)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId, q => q.Include(q => q.User));
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบข้อมูลผู้ขาย"));

            seller.IsVerified = isVerified; // true = อนุมัติ, false = ไม่อนุมัติ/แบน
            await _sellerRepo.UpdateAsync(seller);

            var statusMsg = isVerified ? "ยืนยันตัวตนผู้ขายสำเร็จ" : "ยกเลิกการยืนยันตัวตนผู้ขายแล้ว";
            return Ok(ApiResponse<string>.Success(statusMsg));
        }

        // ผู้ขายอัปเดตข้อมูลส่วนตัว (เช่น เปลี่ยนที่อยู่)
        [HttpPut("update/{sellerId}")]
        public async Task<IActionResult> Update([FromBody] SellerUpdateDto sellerDto, int sellerId)
        {
            // 🌟 ปรับแก้: รับพารามิเตอร์ให้ตรงกับ URL
            var seller = await _sellerRepo.GetByIdAsync(sellerId);
            if (seller == null) return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณต้องการแก้ไข"));

            _mapper.Map(sellerDto, seller);
            await _sellerRepo.UpdateAsync(seller);

            var result = _mapper.Map<SellerDto>(seller);
            return Ok(ApiResponse<SellerDto>.Success(result, "อัปเดตข้อมูลผู้ขายเรียบร้อย"));
        }

        [HttpDelete("delete/{sellerId}")]
        public async Task<IActionResult> Delete(int sellerId)
        {
            var seller = await _sellerRepo.GetByIdAsync(sellerId);
            if (seller == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบผู้ขาย ID: {sellerId}"));

            // 🌟 (Optional) ถ้าลบ Seller ควรเปลี่ยน Role ของเขากลับเป็น Buyer ด้วย
            var user = await _userManager.FindByIdAsync(seller.UserId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles);
                await _userManager.AddToRoleAsync(user, SD.Role_Buyer);
            }

            await _sellerRepo.DeleteAsync(sellerId);

            return Ok(ApiResponse<string>.Success("ลบข้อมูลผู้ขายและปรับสิทธิ์กลับเป็นผู้ใช้งานทั่วไปเรียบร้อยแล้ว"));
        }
    }
}