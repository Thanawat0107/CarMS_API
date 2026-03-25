using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly IRepository<Loan> _loanRepo;
        private readonly IRepository<Car> _carRepo;
        private readonly IMapper _mapper;

        public LoansController(
            IRepository<Loan> loanRepo,
            IRepository<Car> carRepo,
            IMapper mapper)
        {
            _loanRepo = loanRepo;
            _carRepo = carRepo;
            _mapper = mapper;
        }

        // 🌟 ดึงข้อมูลสินเชื่อ (รองรับการกรองด้วย carId หรือ userId ได้)
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int? carId, string? userId, int pageNumber = 1, int pageSize = 10)
        {
            var (loans, totalCount) = await _loanRepo.GetAllAsync(
                filter: q => (!carId.HasValue || q.CarId == carId.Value) &&
                             (string.IsNullOrEmpty(userId) || q.UserId == userId),
                include: query => query.Include(q => q.User).Include(q => q.Car),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            // เรียงลำดับคำขอใหม่ล่าสุดขึ้นก่อน
            var sortedLoans = loans.OrderByDescending(l => l.CreatedAt).ToList();
            var result = _mapper.Map<IEnumerable<LoanDto>>(sortedLoans);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<LoanDto>>.Success(result, "โหลดข้อมูลคำขอสินเชื่อสำเร็จ", meta));
        }

        [HttpGet("getbyid/{loanId}")]
        public async Task<IActionResult> GetById(int loanId)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId, q => q.Include(l => l.User).Include(l => l.Car));
            if (loan == null) return NotFound(ApiResponse<string>.Fail("ไม่พบคำขอประเมินสินเชื่อที่คุณค้นหา"));

            var result = _mapper.Map<LoanDto>(loan);
            return Ok(ApiResponse<LoanDto>.Success(result, "สำเร็จ"));
        }

        // ลูกค้าส่งฟอร์มขอสินเชื่อ
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LoanCreateDto loanDto)
        {
            var car = await _carRepo.GetByIdAsync(loanDto.CarId);
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถยนต์ที่คุณต้องการขอสินเชื่อ"));

            // 🌟 ป้องกันการสแปม: เช็คว่าเคยยื่นขอไปแล้วและสถานะยัง Pending อยู่หรือไม่
            var existingLoan = await _loanRepo.FirstOrDefaultAsync(l => 
                l.UserId == loanDto.UserId && 
                l.CarId == loanDto.CarId &&
                l.LoanStatus == SD.Loan_Pending);

            if (existingLoan != null)
                return BadRequest(ApiResponse<string>.Fail("คุณได้ยื่นคำขอสินเชื่อสำหรับรถคันนี้ไปแล้ว (โปรดรอผู้ขายติดต่อกลับ)"));

            var loan = _mapper.Map<Loan>(loanDto);
            loan.CreatedAt = DateTime.UtcNow;
            loan.LoanStatus = SD.Loan_Pending; 

            var created = await _loanRepo.AddAsync(loan);
            var result = _mapper.Map<LoanDto>(created);

            return Ok(ApiResponse<LoanDto>.Success(result, "ส่งคำขอประเมินสินเชื่อเบื้องต้นเรียบร้อยแล้ว ผู้ขายจะติดต่อกลับในภายหลัง"));
        }

        // ผู้ขายหรือ Admin อัปเดตสถานะ (เช่น ติดต่อลูกค้าแล้ว หรือ ถูกปฏิเสธ)
        [HttpPut("update-status/{loanId}")]
        public async Task<IActionResult> UpdateStatus(int loanId, [FromBody] LoanUpdateDto updateDto)
        {
            var loan = await _loanRepo.GetByIdAsync(loanId);
            if (loan == null) return NotFound(ApiResponse<string>.Fail("ไม่พบคำขอสินเชื่อที่ต้องการแก้ไข"));

            loan.LoanStatus = updateDto.LoanStatus; // เช่น SD.Loan_Contacted หรือ SD.Loan_Rejected
            
            await _loanRepo.UpdateAsync(loan);
            
            var result = _mapper.Map<LoanDto>(loan);
            return Ok(ApiResponse<LoanDto>.Success(result, "อัปเดตสถานะคำขอสินเชื่อเรียบร้อย"));
        }

        [HttpDelete("delete/{loanId}")]
        public async Task<IActionResult> Delete(int loanId)
        {
            var deleted = await _loanRepo.DeleteAsync(loanId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail("ไม่พบคำขอสินเชื่อที่ต้องการลบ"));

            return Ok(ApiResponse<string>.Success("ลบคำขอประเมินสินเชื่อสำเร็จ"));
        }
    }
}