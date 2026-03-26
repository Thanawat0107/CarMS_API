using AutoMapper;
using CarMS_API.Hubs;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdateDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<NotificationHub> _hubContext;

        public LoansController(
            IRepository<Loan> loanRepo,
            IRepository<Car> carRepo,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)

        {
            _loanRepo = loanRepo;
            _carRepo = carRepo;
            _mapper = mapper;
            _hubContext = hubContext;

        }

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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] LoanCreateDto loanDto)
        {
            // Include ข้อมูลผู้ขายและยี่ห้อรถมาเพื่อใช้ในการแจ้งเตือน
            var car = await _carRepo.GetByIdAsync(loanDto.CarId, q => q.Include(c => c.Seller).Include(c => c.Brand));
            if (car == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรถยนต์ที่คุณต้องการขอสินเชื่อ"));

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

            // ⚡ สั่ง SignalR: ยิงแจ้งเตือนไปหา "ผู้ขาย (Seller.UserId)"
            if (car.Seller != null && !string.IsNullOrEmpty(car.Seller.UserId))
            {
                var notificationMessage = new 
                {
                    Title = "ลูกค้าขอประเมินสินเชื่อ! 💸",
                    Message = $"รถ {car.Brand?.Name} {car.Model} มีผู้สนใจจัดไฟแนนซ์ (ดาวน์ {loan.DownPayment:N0} บาท) โปรดตรวจสอบข้อมูล",
                    LoanId = created.Id
                };
                await _hubContext.Clients.Group(car.Seller.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

            return Ok(ApiResponse<LoanDto>.Success(result, "ส่งคำขอประเมินสินเชื่อเบื้องต้นเรียบร้อยแล้ว ผู้ขายจะติดต่อกลับในภายหลัง"));
        }

        [HttpPut("update-status/{loanId}")]
        public async Task<IActionResult> UpdateStatus(int loanId, [FromBody] LoanUpdateDto updateDto)
        {
            // Include ข้อมูลรถมาเพื่อบอกลูกค้าว่าอัปเดตของรถคันไหน
            var loan = await _loanRepo.GetByIdAsync(loanId, q => q.Include(l => l.Car));
            if (loan == null) return NotFound(ApiResponse<string>.Fail("ไม่พบคำขอสินเชื่อที่ต้องการแก้ไข"));

            loan.LoanStatus = updateDto.LoanStatus; 
            await _loanRepo.UpdateAsync(loan);
            
            var result = _mapper.Map<LoanDto>(loan);

            // ⚡ สั่ง SignalR: ยิงแจ้งเตือนไปหา "ลูกค้า (UserId)"
            if (!string.IsNullOrEmpty(loan.UserId))
            {
                var notificationMessage = new 
                {
                    Title = "อัปเดตคำขอสินเชื่อของคุณ 📋",
                    Message = $"คำขอสินเชื่อรถ {loan.Car?.Model} ถูกเปลี่ยนสถานะเป็น: {loan.LoanStatus}",
                    LoanId = loan.Id
                };
                await _hubContext.Clients.Group(loan.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

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