using Microsoft.AspNetCore.SignalR;

namespace CarMS_API.Hubs
{
    public class ChatHub : Hub
    {
        // เมื่อ Frontend (Next.js) เชื่อมต่อสำเร็จ จะส่ง UserId ของตัวเองมาที่ฟังก์ชันนี้
        // เพื่อเอาตัวเองเข้าไปอยู่ใน "ห้องส่วนตัว" (Group) ที่ตั้งชื่อตาม UserId
        public async Task JoinPersonalRoom(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        // (Optional) เผื่อใช้ตอนออกจากหน้าระบบ
        public async Task LeavePersonalRoom(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
}