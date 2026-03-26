using Microsoft.AspNetCore.SignalR;

namespace CarMS_API.Hubs
{
    public class NotificationHub : Hub
    {
        // ให้ User เข้ามารับการแจ้งเตือนส่วนตัว
        public async Task JoinNotificationRoom(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        public async Task LeaveNotificationRoom(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
}