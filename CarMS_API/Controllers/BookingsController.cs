using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Models;
using CarMS_API.Utility;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Models.Dto.CreateDto;
using Microsoft.AspNetCore.SignalR;
using CarMS_API.Hubs;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IRepository<Booking> _BookingRepo;
        private readonly IRepository<Car> _carRepo;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;
        
        public BookingsController(
            IRepository<Booking> BookingRepo,
            IRepository<Car> carRepo,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext)
        {
            _BookingRepo = BookingRepo;
            _carRepo = carRepo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(string? userId, int? sellerId, int pageNumber = 1, int pageSize = 10)
        {
            var (bookings, totalCount) = await _BookingRepo.GetAllAsync(
                filter: q => 
                    (string.IsNullOrEmpty(userId) || q.UserId == userId) &&
                    (!sellerId.HasValue || q.Car.SellerId == sellerId.Value),
                include: query => query.Include(q => q.Car).Include(q => q.User),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<BookingDto>>(bookings);
            var meta = new PaginationMeta { TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };

            return Ok(ApiResponse<IEnumerable<BookingDto>>.Success(result, "เรียกดูรายการจองสำเร็จ", meta));
        }

        [HttpGet("getbyid/{BookingId}")]
        public async Task<IActionResult> GetById(int BookingId)
        {
            var Booking = await _BookingRepo.GetByIdAsync(BookingId, q => q.Include(q => q.User).Include(q => q.Car));
            if (Booking == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการจองรถ"));
            return Ok(ApiResponse<BookingDto>.Success(_mapper.Map<BookingDto>(Booking), "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(BookingCreateDto BookingDto)
        {
            var Booking = _mapper.Map<Booking>(BookingDto);
            Booking.ReservedAt = DateTime.UtcNow;
            Booking.ExpiryAt = Booking.ReservedAt.AddHours(24);
            Booking.BookingStatus = SD.Booking_Pending;

            var activeBooking = await _BookingRepo.FirstOrDefaultAsync(r =>
                r.CarId == Booking.CarId &&
                (r.BookingStatus == SD.Booking_Pending || r.BookingStatus == SD.Booking_PendingPayment || r.BookingStatus == SD.Booking_Confirmed));

            if (activeBooking != null) 
                return BadRequest(ApiResponse<string>.Fail(activeBooking.UserId == Booking.UserId ? "คุณได้จองรถคันนี้ไว้แล้ว" : "ขออภัย รถคันนี้อยู่ระหว่างการจองของลูกค้ารายอื่น"));

            var car = await _carRepo.GetByIdAsync(Booking.CarId, q => q.Include(c => c.Seller));
            if (car == null || car.CarStatus != SD.Status_Available) 
                return BadRequest(ApiResponse<string>.Fail("รถไม่พร้อมให้จอง"));

            car.CarStatus = SD.Status_Booked;
            await _carRepo.UpdateAsync(car);

            var created = await _BookingRepo.AddAsync(Booking);
            var result = _mapper.Map<BookingCreateDto>(created);

            // ⚡ 1. แจ้งเตือนคนขาย (ส่วนตัว)
            if (car.Seller != null && !string.IsNullOrEmpty(car.Seller.UserId))
            {
                var notificationMessage = new 
                {
                    Title = "มีคำสั่งจองรถใหม่!",
                    Message = $"รถ {car.Brand?.Name} {car.Model} ของคุณถูกจองแล้ว กรุณารอผู้ซื้อชำระเงิน",
                    BookingId = created.Id
                };
                await _hubContext.Clients.Group(car.Seller.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

            // ⚡ 2. ประกาศให้ทุกคนในเว็บรู้ว่ารถคันนี้ "ถูกจองแล้ว" (Broadcast)
            await _hubContext.Clients.All.SendAsync("CarStatusChanged", new 
            {
                CarId = car.Id,
                NewStatus = SD.Status_Booked
            });

            return Ok(ApiResponse<BookingCreateDto>.Success(result, "จองรถสำเร็จ กรุณาชำระเงินภายใน 24 ชั่วโมง"));
        }

        [HttpPut("cancel/{BookingId}")]
        public async Task<IActionResult> Cancel(int BookingId)
        {
            var Booking = await _BookingRepo.GetByIdAsync(BookingId, r => r.Include(r => r.Car).ThenInclude(c => c.Seller));
            
            if (Booking == null || (Booking.BookingStatus != SD.Booking_Pending && Booking.BookingStatus != SD.Booking_PendingPayment))
                return NotFound(ApiResponse<string>.Fail("รายการนี้ไม่สามารถยกเลิกได้"));

            Booking.BookingStatus = SD.Booking_Canceled;
            Booking.CanceledAt = DateTime.UtcNow;

            if (Booking.Car != null)
            {
                Booking.Car.CarStatus = SD.Status_Available;
            }

            await _BookingRepo.UpdateAsync(Booking);
            var result = _mapper.Map<BookingCreateDto>(Booking);

            // ⚡ 1. แจ้งเตือนคนขาย (ส่วนตัว)
            if (Booking.Car?.Seller != null && !string.IsNullOrEmpty(Booking.Car.Seller.UserId))
            {
                var notificationMessage = new 
                {
                    Title = "ลูกค้าขอยกเลิกการจอง",
                    Message = $"การจองรถ {Booking.Car.Model} ถูกยกเลิก สถานะรถกลับมาว่างอีกครั้ง",
                    BookingId = Booking.Id
                };
                await _hubContext.Clients.Group(Booking.Car.Seller.UserId).SendAsync("ReceiveNotification", notificationMessage);
            }

            // ⚡ 2. ประกาศให้ทุกคนในเว็บรู้ว่ารถคันนี้ "หลุดจอง กลับมาว่างแล้ว" (Broadcast)
            await _hubContext.Clients.All.SendAsync("CarStatusChanged", new 
            {
                CarId = Booking.CarId,
                NewStatus = SD.Status_Available
            });

            return Ok(ApiResponse<BookingCreateDto>.Success(result, "ยกเลิกการจองสำเร็จ"));
        }

        [HttpDelete("delete/{BookingId}")]
        public async Task<IActionResult> Delete(int BookingId)
        {
            var booking = await _BookingRepo.GetByIdAsync(BookingId, r => r.Include(r => r.Car));
            if (booking == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการจอง"));

            if (booking.Car != null && booking.Car.CarStatus == SD.Status_Booked)
            {
                booking.Car.CarStatus = SD.Status_Available;
                await _carRepo.UpdateAsync(booking.Car);
            }

            await _BookingRepo.DeleteAsync(BookingId);
            return Ok(ApiResponse<string>.Success("ลบรายการจองรถเรียบร้อยแล้ว"));
        }
    }
}