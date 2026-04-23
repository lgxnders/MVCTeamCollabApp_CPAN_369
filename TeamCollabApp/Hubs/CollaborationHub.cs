using Microsoft.AspNetCore.SignalR;

namespace TeamCollabApp.Hubs
{
    public class CollaborationHub : Hub
    {
        public async Task JoinProject(int projectId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");

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

        public async Task AnnouncePresence(int projectId, string displayName) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("UserJoined", displayName);

        public async Task AnnounceLeave(int projectId, string displayName) =>
            await Clients.OthersInGroup($"project-{projectId}").SendAsync("UserLeft", displayName);

        public async Task JoinProjectDetails(int projectId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"details-{projectId}");
    }
}
