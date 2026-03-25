using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdateDto; // ถ้าสร้างไว้
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Seller> _sellerRepo;
        private readonly IMapper _mapper;

        public ReviewsController(
            IRepository<Review> reviewRepo,
            IRepository<Seller> sellerRepo,
            IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _sellerRepo = sellerRepo;
            _mapper = mapper;
        }

        // ดึงรีวิวทั้งหมด (ถ้าส่ง sellerId มา จะดึงเฉพาะของคนขายคนนั้น)
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int? sellerId, int pageNumber = 1, int pageSize = 10)
        {
            var (reviews, totalCount) = await _reviewRepo.GetAllAsync(
                filter: q => !sellerId.HasValue || q.SellerId == sellerId.Value, // 🌟 กรองตามคนขาย
                include: query => query.Include(q => q.User), // ดึงข้อมูลคนคอมเมนต์มาด้วย
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            // เรียงลำดับรีวิวใหม่ล่าสุดขึ้นก่อน
            var sortedReviews = reviews.OrderByDescending(r => r.CreatedAt).ToList();

            var result = _mapper.Map<IEnumerable<ReviewDto>>(sortedReviews);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<ReviewDto>>.Success(result, "โหลดรายการรีวิวสำเร็จ", meta));
        }

        [HttpGet("getbyid/{reviewId}")]
        public async Task<IActionResult> GetById(int reviewId)
        {
            var review = await _reviewRepo.GetByIdAsync(reviewId, q => q.Include(r => r.User));
            if (review == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรีวิวที่คุณค้นหา"));
            
            var result = _mapper.Map<ReviewDto>(review);
            return Ok(ApiResponse<ReviewDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto reviewDto)
        {
            // 1. ดักจับคะแนน ต้องอยู่ระหว่าง 1-5 เท่านั้น
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
                return BadRequest(ApiResponse<string>.Fail("คะแนนรีวิวต้องอยู่ระหว่าง 1 ถึง 5 ดาวเท่านั้น"));

            // 2. เช็คว่า SellerId ไม่เป็น null
            if (!reviewDto.SellerId.HasValue)
                return BadRequest(ApiResponse<string>.Fail("ต้องระบุรหัสผู้ขาย"));

            // 3. เช็คว่ามีผู้ขายคนนี้อยู่จริงไหม
            var seller = await _sellerRepo.GetByIdAsync(reviewDto.SellerId.Value);
            if (seller == null)
                return NotFound(ApiResponse<string>.Fail("ไม่พบผู้ขายที่คุณต้องการรีวิว"));

            // 4. ป้องกันการสแปม (สมมติให้ 1 User รีวิว Seller 1 คนได้แค่ครั้งเดียว)
            var existingReview = await _reviewRepo.FirstOrDefaultAsync(r => 
                r.UserId == reviewDto.UserId && r.SellerId == reviewDto.SellerId);
            
            if (existingReview != null)
                return BadRequest(ApiResponse<string>.Fail("คุณได้รีวิวผู้ขายรายนี้ไปแล้ว"));

            var review = _mapper.Map<Review>(reviewDto);
            review.CreatedAt = DateTime.UtcNow;

            var created = await _reviewRepo.AddAsync(review);
            var result = _mapper.Map<ReviewDto>(created);

            return Ok(ApiResponse<ReviewDto>.Success(result, "ส่งรีวิวสำเร็จ ขอบคุณสำหรับความคิดเห็นครับ"));
        }

        [HttpPut("update/{reviewId}")]
        public async Task<IActionResult> Update([FromBody] ReviewUpdateDto updateDto, int reviewId)
        {
            if (updateDto.Rating < 1 || updateDto.Rating > 5)
                return BadRequest(ApiResponse<string>.Fail("คะแนนรีวิวต้องอยู่ระหว่าง 1 ถึง 5 ดาวเท่านั้น"));

            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรีวิวที่คุณต้องการแก้ไข"));

            _mapper.Map(updateDto, review);
            // ถ้าอยากบันทึกเวลาที่แก้ไข สามารถเพิ่ม review.UpdatedAt = DateTime.UtcNow; (และไปเติมฟิลด์ใน Model)

            await _reviewRepo.UpdateAsync(review);
            
            var result = _mapper.Map<ReviewDto>(review);
            return Ok(ApiResponse<ReviewDto>.Success(result, "แก้ไขรีวิวสำเร็จ"));
        }

        [HttpDelete("delete/{reviewId}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var deleted = await _reviewRepo.DeleteAsync(reviewId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรีวิวที่ต้องการลบ"));

            return Ok(ApiResponse<string>.Success("ลบรีวิวสำเร็จ"));
        }
    }
}