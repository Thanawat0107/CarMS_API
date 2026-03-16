using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Models;
using CarMS_API.Utility;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Models.Dto.CreateDto;

namespace BookingMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IRepository<Booking> _BookingRepo;
        private readonly IRepository<Car> _carRepo;
        private readonly IMapper _mapper;
        public BookingsController(
            IRepository<Booking> BookingRepo,
            IRepository<Car> carRepo,
            IMapper mapper)
        {
            _BookingRepo = BookingRepo;
            _carRepo = carRepo;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (bookings, totalCount) = await _BookingRepo.GetAllAsync(
                include: query => query
                    .Include(q => q.Car),
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<BookingDto>>(bookings);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<BookingDto>>.Success(result, "เรียกดูรายการจองสำเร็จ", meta));
        }

        [HttpGet("getbyid/{BookingId}")]
        public async Task<IActionResult> GetById(int BookingId)
        {
            var Booking = await _BookingRepo.GetByIdAsync(BookingId,
                q => q.Include(q => q.User)
                .Include(q => q.Car));

            if (Booking == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการจองรถที่คุณค้นหา"));
            var result = _mapper.Map<BookingDto>(Booking);

            return Ok(ApiResponse<BookingDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(BookingCreateDto BookingDto)
        {
            var Booking = _mapper.Map<Booking>(BookingDto);
            Booking.ReservedAt = DateTime.UtcNow;
            Booking.ExpiryAt = Booking.ReservedAt.AddHours(24);
            Booking.BookingStatus = SD.Reserve_Pending;

            var existing = await _BookingRepo.FirstOrDefaultAsync(r =>
                r.CarId == Booking.CarId &&
                r.UserId == Booking.UserId &&
                r.BookingStatus == SD.Reserve_Pending
            );

            if (existing != null) return BadRequest(ApiResponse<string>.Fail("คุณได้จองรถคันนี้ไว้แล้ว"));

            var car = await _carRepo.GetByIdAsync(Booking.CarId);
            if (car == null || car.CarStatus != SD.Status_Available) return BadRequest(ApiResponse<string>.Fail("รถไม่พร้อมให้จอง"));

            car.CarStatus = SD.Status_Booked;
            await _carRepo.UpdateAsync(car);

            var created = await _BookingRepo.AddAsync(Booking);
            var result = _mapper.Map<BookingCreateDto>(created);

            return Ok(ApiResponse<BookingCreateDto>.Success(result, "จองรถสำเร็จ รอการยืนยัน"));
        }

        [HttpPut("cancel/{BookingId}")]
        public async Task<IActionResult> Cancel(int BookingId)
        {
            var Booking = await _BookingRepo.GetByIdAsync(BookingId, r => r.Include(r => r.Car));
            if (Booking == null || Booking.BookingStatus != SD.Reserve_Pending)
                return NotFound(ApiResponse<string>.Fail("ไม่สามารถยกเลิกได้"));

            Booking.BookingStatus = SD.Reserve_Canceled;
            if (Booking.Car != null)
            {
                Booking.Car.CarStatus = SD.Status_Available;
            }

            Booking.CanceledAt = DateTime.UtcNow;

            await _BookingRepo.UpdateAsync(Booking);
            var result = _mapper.Map<BookingCreateDto>(Booking);
            return Ok(ApiResponse<BookingCreateDto>.Success(result, "ยกเลิกการจองสำเร็จ"));
        }

        [HttpDelete("delete/{BookingId}")]
        public async Task<IActionResult> Delete(int BookingId)
        {
            await _BookingRepo.DeleteAsync(BookingId);
            return Ok(ApiResponse<string>.Success("ลบรายการจองรถเรียบร้อยแล้ว"));
        }
    }
}