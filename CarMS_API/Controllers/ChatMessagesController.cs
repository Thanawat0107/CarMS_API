using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto; // สมมติว่ามี ChatMessageDto
using CarMS_API.Models.Dto.CreateDto; // สมมติว่ามี ChatMessageCreateDto
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessagesController : ControllerBase
    {
        private readonly IRepository<ChatMessage> _chatRepo;
        private readonly IMapper _mapper;

        public ChatMessagesController(IRepository<ChatMessage> chatRepo, IMapper mapper)
        {
            _chatRepo = chatRepo;
            _mapper = mapper;
        }

        // ดึงข้อความแชททั้งหมด (เรียงจากใหม่ไปเก่า)
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 50) // แชทมักจะโหลดทีละเยอะๆ
        {
            var (messages, totalCount) = await _chatRepo.GetAllAsync(
                include: q => q.Include(u => u.Sender), // ดึงข้อมูลคนส่งมาด้วย
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            // 🌟 ทริค: แชทมักจะเรียงข้อความล่าสุดไว้บนสุด หรือล่างสุด (อันนี้จัดให้แบบล่าสุดอยู่บน)
            var sortedMessages = messages.OrderByDescending(m => m.CreatedAt).ToList();

            var result = _mapper.Map<IEnumerable<ChatMessageDto>>(sortedMessages);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<ChatMessageDto>>.Success(result, "โหลดข้อความแชทสำเร็จ", meta));
        }

        // ส่งข้อความใหม่
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageCreateDto chatDto)
        {
            if (string.IsNullOrWhiteSpace(chatDto.Message))
                return BadRequest(ApiResponse<string>.Fail("ข้อความไม่สามารถเป็นค่าว่างได้"));

            var chatMessage = _mapper.Map<ChatMessage>(chatDto);
            
            chatMessage.CreatedAt = DateTime.UtcNow;
            chatMessage.IsRead = false; // ส่งปุ๊บ สถานะคือยังไม่ได้อ่าน

            var created = await _chatRepo.AddAsync(chatMessage);
            var result = _mapper.Map<ChatMessageDto>(created);

            return Ok(ApiResponse<ChatMessageDto>.Success(result, "ส่งข้อความสำเร็จ"));
        }

        // อัปเดตสถานะว่า "อ่านแล้ว" (Read Receipt)
        [HttpPut("markasread/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _chatRepo.GetByIdAsync(messageId);
            if (message == null) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบข้อความ"));

            if (!message.IsRead)
            {
                message.IsRead = true;
                await _chatRepo.UpdateAsync(message);
            }

            return Ok(ApiResponse<string>.Success("อ่านข้อความแล้ว"));
        }

        // ลบข้อความ (Unsend)
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var deleted = await _chatRepo.DeleteAsync(messageId);
            if (deleted == null) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบข้อความที่ต้องการลบ"));

            return Ok(ApiResponse<string>.Success("ลบข้อความสำเร็จ"));
        }
    }
}