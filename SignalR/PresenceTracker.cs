namespace AngularMaterialApi.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers =
            new Dictionary<string, List<string>>();
        private static readonly Dictionary<string, string> userRoutes = new();
        private static readonly Dictionary<string, string> connectionToUser = new(); // Connection ID to Username mapping

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);
                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }

            userRoutes.Remove(connectionId);
            connectionToUser.Remove(connectionId);

            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }

        public Task UpdateUserRoute(string username, string connectionId, string route)
        {
            lock (userRoutes)
            {
                userRoutes[connectionId] = route;
                connectionToUser[connectionId] = username; // Store username to connection mapping
            }

            return Task.CompletedTask;
        }

        public Task<List<string>> GetUsersOnPage(string route)
        {
            var usersOnPage = new List<string>();

            lock (userRoutes)
            {
                usersOnPage = userRoutes
                    .Where(kv => kv.Value == route)
                    .Select(kv => connectionToUser[kv.Key]) // Map connection ID to username
                    .Distinct()
                    .ToList(); // Ensure it's a list
            }

            return Task.FromResult(usersOnPage);
        }

        public Task<string> GetUserRoute(string connectionId)
        {
            string route;
            lock (userRoutes)
            {
                userRoutes.TryGetValue(connectionId, out route);
            }
            return Task.FromResult(route ?? string.Empty);
        }
    }
}
