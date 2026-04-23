using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace TeamCollabApp.Hubs
{
    public class CollaborationHub : Hub
    {
        // Maps connectionId -> (projectId, displayName) so we can clean up on disconnect.
        private static readonly ConcurrentDictionary<string, (int ProjectId, string DisplayName)> _presenceConnections = new();

        // Maps projectId -> set of display names currently online.
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, int>> _projectPresence = new();

        public async Task JoinProject(int projectId, string displayName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");

            _presenceConnections[Context.ConnectionId] = (projectId, displayName);
            var members = _projectPresence.GetOrAdd(projectId, _ => new ConcurrentDictionary<string, int>());
            members.AddOrUpdate(displayName, 1, (_, count) => count + 1);

            // Send the current presence list to the caller only.
            await Clients.Caller.SendAsync("PresenceList", members.Keys.ToList());

            // Notify everyone else that this user joined.
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("UserJoined", displayName);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_presenceConnections.TryRemove(Context.ConnectionId, out var info))
            {
                if (_projectPresence.TryGetValue(info.ProjectId, out var members))
                {
                    members.AddOrUpdate(info.DisplayName, 0, (_, count) => count - 1);
                    if (members.TryGetValue(info.DisplayName, out var remaining) && remaining <= 0)
                    {
                        members.TryRemove(info.DisplayName, out _);
                        await Clients.Group($"project-{info.ProjectId}").SendAsync("UserLeft", info.DisplayName);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task LeaveProject(int projectId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");

        public async Task SendDocumentDelta(int projectId, object delta) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("ReceiveDocumentDelta", delta);

        public async Task CommentAdded(int projectId, object comment) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnCommentAdded", comment);

        public async Task CommentResolved(int projectId, string commentKey) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnCommentResolved", commentKey);

        public async Task TaskMoved(int projectId, object payload) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnTaskMoved", payload);

        public async Task TaskCreated(int projectId, object task) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnTaskCreated", task);

        public async Task TaskUpdated(int projectId, object task) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnTaskUpdated", task);

        public async Task TaskDeleted(int projectId, int taskId) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("OnTaskDeleted", taskId);

        public async Task JoinProjectDetails(int projectId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"details-{projectId}");
    }
}
