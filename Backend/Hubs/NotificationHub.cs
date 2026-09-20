using Microsoft.AspNetCore.SignalR;

namespace PaymentReminder.Api.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task SendNotification(string userId, string message, string type = "info")
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", message, type);
        }
    }
}