using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Models.Dto.CreateDto;

namespace ReservationMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IRepository<Reservation> _reservationRepo;
        private readonly IRepository<Car> _carRepo;
        private readonly ISearchableRepository<Reservation, ReservationSearchParams> _searchRepo;
        private readonly IMapper _mapper;
        public ReservationsController(
            IRepository<Reservation> reservationRepo,
            ISearchableRepository<Reservation, 
            ReservationSearchParams> searchRepo,
            IRepository<Car> carRepo,
            IMapper mapper)
        {
            _reservationRepo = reservationRepo;
            _searchRepo = searchRepo;
            _carRepo = carRepo;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll([FromQuery] ReservationSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (reservations, totalCount) = await _reservationRepo.GetAllAsync(
                filter,
                orderBy,
                include: _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<ReservationDto>>(reservations);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<ReservationDto>>.Success(result, "โหลดรายการจองรถเรียบร้อย", pagination));
        }

        [HttpGet("getbyid/{reservationId}")]
        public async Task<IActionResult> GetById(int reservationId)
        {
            var reservation = await _reservationRepo.GetByIdAsync(reservationId,
                q => q.Include(q => q.User)
                .Include(q => q.Car));

            if (reservation == null) return NotFound(ApiResponse<string>.Fail("ไม่พบรายการจองรถที่คุณค้นหา"));
            var result = _mapper.Map<ReservationDto>(reservation);

            return Ok(ApiResponse<ReservationDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(ReservationCreateDto reservationDto)
        {
            var reservation = _mapper.Map<Reservation>(reservationDto);
            reservation.ReservedAt = DateTime.UtcNow;
            reservation.ExpiryAt = reservation.ReservedAt.AddHours(24);
            reservation.Status = ReservationStatus.Pending;

            var existing = await _reservationRepo.FirstOrDefaultAsync(r =>
                r.CarId == reservation.CarId &&
                r.UserId == reservation.UserId &&
                r.Status == ReservationStatus.Pending
            );

            if (existing != null) return BadRequest(ApiResponse<string>.Fail("คุณได้จองรถคันนี้ไว้แล้ว"));

            var car = await _carRepo.GetByIdAsync(reservation.CarId);
            if (car == null || car.Status != Status.Available) return BadRequest(ApiResponse<string>.Fail("รถไม่พร้อมให้จอง"));

            car.Status = Status.Reserved;
            await _carRepo.UpdateAsync(car);

            var created = await _reservationRepo.AddAsync(reservation);
            var result = _mapper.Map<ReservationCreateDto>(created);

            return Ok(ApiResponse<ReservationCreateDto>.Success(result, "จองรถสำเร็จ รอการยืนยัน"));
        }

        [HttpPut("cancel/{reservationId}")]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            var reservation = await _reservationRepo.GetByIdAsync(reservationId, r => r.Include(r => r.Car));
            if (reservation == null || reservation.Status != ReservationStatus.Pending)
                return NotFound(ApiResponse<string>.Fail("ไม่สามารถยกเลิกได้"));

            reservation.Status = ReservationStatus.Canceled;
            if (reservation.Car != null)
            {
                reservation.Car.Status = Status.Available;
            }

            reservation.CanceledAt = DateTime.UtcNow;

            await _reservationRepo.UpdateAsync(reservation);
            var result = _mapper.Map<ReservationCreateDto>(reservation);
            return Ok(ApiResponse<ReservationCreateDto>.Success(result, "ยกเลิกการจองสำเร็จ"));
        }

        [HttpDelete("delete/{reservationId}")]
        public async Task<IActionResult> Delete(int reservationId)
        {
            await _reservationRepo.DeleteAsync(reservationId);
            return Ok(ApiResponse<string>.Success("ลบรายการจองรถเรียบร้อยแล้ว"));
        }
    }
}