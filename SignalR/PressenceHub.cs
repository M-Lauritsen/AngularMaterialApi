using AngularMaterialApi.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AngularMaterialApi.SignalR
{
    [Authorize]
    public class PressenceHub(PressencTracker _pressenceTracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _pressenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }

            var currentUsers = await _pressenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _pressenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
