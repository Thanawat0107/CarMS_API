using AutoMapper;
using CarMS_API.Hubs;
using CarMS_API.Models;
using CarMS_API.Models.Dto; // สมมติว่ามี ChatMessageDto
using CarMS_API.Models.Dto.CreateDto; // สมมติว่ามี ChatMessageCreateDto
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessagesController : ControllerBase
    {
        private readonly IRepository<ChatMessage> _chatRepo;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatMessagesController(
            IRepository<ChatMessage> chatRepo, 
            IMapper mapper, 
            IHubContext<ChatHub> hubContext)
        {
            _chatRepo = chatRepo;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // ดึงข้อความแชททั้งหมด (ไม่มีการเปลี่ยนแปลงข้อมูล ไม่ต้องใส่ SignalR)
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 50)
        {
            var (messages, totalCount) = await _chatRepo.GetAllAsync(
                include: q => q.Include(u => u.Sender).Include(u => u.Receiver), 
                pageNumber: pageNumber,
                pageSize: pageSize
            );

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

        // 🌟 จุดที่ 1: ส่งข้อความใหม่
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageCreateDto chatDto)
        {
            if (string.IsNullOrWhiteSpace(chatDto.Message))
                return BadRequest(ApiResponse<string>.Fail("ข้อความไม่สามารถเป็นค่าว่างได้"));

            var chatMessage = _mapper.Map<ChatMessage>(chatDto);
            
            chatMessage.CreatedAt = DateTime.UtcNow;
            chatMessage.IsRead = false; // ส่งปุ๊บ สถานะคือยังไม่ได้อ่าน

            var created = await _chatRepo.AddAsync(chatMessage);

            // ดึงข้อมูลคนส่งและคนรับติดมาด้วย เพื่อให้หน้าเว็บเอาไปโชว์ได้เลย
            var fullMessage = await _chatRepo.GetByIdAsync(created.Id, 
                q => q.Include(u => u.Sender).Include(u => u.Receiver));
                
            var result = _mapper.Map<ChatMessageDto>(fullMessage);

            // ⚡ สั่ง SignalR: ส่งข้อความไปที่หน้าจอของ "คนรับ (ReceiverId)" ทันที
            await _hubContext.Clients.Group(chatDto.ReceiverId).SendAsync("ReceiveNewMessage", result);
            
            // (Optional) ส่งกลับไปที่คนส่งด้วย เผื่อเขาเปิดเว็บไว้หลายแท็บ จะได้อัปเดตตรงกัน
            await _hubContext.Clients.Group(chatDto.SenderId).SendAsync("ReceiveNewMessage", result);

            return Ok(ApiResponse<ChatMessageDto>.Success(result, "ส่งข้อความสำเร็จ"));
        }

        // 🌟 จุดที่ 2: อัปเดตสถานะว่า "อ่านแล้ว" 
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

                // ⚡ สั่ง SignalR: แจ้งเตือน "คนส่ง (SenderId)" ว่าข้อความนี้ถูกอ่านแล้วนะ
                await _hubContext.Clients.Group(message.SenderId).SendAsync("MessageRead", messageId);
            }

            return Ok(ApiResponse<string>.Success("อ่านข้อความแล้ว"));
        }

        // 🌟 จุดที่ 3: ลบข้อความ (Unsend)
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            // ดึงข้อมูลมาก่อนลบ เพื่อให้รู้ว่าข้อความนี้ส่งไปหาใคร จะได้สั่งลบถูกคน
            var message = await _chatRepo.GetByIdAsync(messageId);
            if (message == null) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบข้อความที่ต้องการลบ"));

            await _chatRepo.DeleteAsync(messageId);

            // ⚡ สั่ง SignalR: บอกให้หน้าจอของ "คนรับ" และ "คนส่ง" ลบข้อความนี้ออกจากหน้าจอซะ
            await _hubContext.Clients.Group(message.ReceiverId).SendAsync("MessageDeleted", messageId);
            await _hubContext.Clients.Group(message.SenderId).SendAsync("MessageDeleted", messageId);

            return Ok(ApiResponse<string>.Success("ลบข้อความสำเร็จ"));
        }
    }
}