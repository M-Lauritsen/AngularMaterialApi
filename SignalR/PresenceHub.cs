using AngularMaterialApi.Extension;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AngularMaterialApi.SignalR
{
    [Authorize]
    public class PresenceHub(PresenceTracker _presenceTracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var isOnline = await _presenceTracker.UserConnected(Context.User?.GetUsername(), Context.ConnectionId);
            if (isOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());
            }

            var currentUsers = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _presenceTracker.UserDisconnected(Context.User?.GetUsername(), Context.ConnectionId);

            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateUserRoute(string route)
        {
            var connectionId = Context.ConnectionId;
            var username = Context.User?.GetUsername();

            var oldRoute = await _presenceTracker.GetUserRoute(connectionId);
            if (!string.IsNullOrEmpty(oldRoute) && oldRoute != route)
            {
                await Groups.RemoveFromGroupAsync(connectionId, oldRoute);
            }

            await Groups.AddToGroupAsync(connectionId, route);
            await _presenceTracker.UpdateUserRoute(username, connectionId, route);

            var usersOnPage = await _presenceTracker.GetUsersOnPage(route);
            await Clients.Group(route).SendAsync("UsersOnPage", usersOnPage);

            // Update users on the old page
            if (!string.IsNullOrEmpty(oldRoute) && oldRoute != route)
            {
                var usersOnOldPage = await _presenceTracker.GetUsersOnPage(oldRoute);
                await Clients.Group(oldRoute).SendAsync("UsersOnPage", usersOnOldPage);
            }
        }
    }
}
