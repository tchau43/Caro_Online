using Microsoft.AspNetCore.SignalR;

namespace Server
{
    public class ChatHub : Hub
    {
        private static int maxParticipants = 2;
        private static List<string> participants = new List<string>();
        private static EStatus currentTurn = EStatus.X;

        public async Task JoinChat()
        {
            if (participants.Count < maxParticipants)
            {
                participants.Add(Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
                await Clients.Groups("ChatRoom").SendAsync("UserJoined", Context.ConnectionId);
                await Clients.Client(Context.ConnectionId).SendAsync("NotiStatus", participants.Count == 1 ? EStatus.X : EStatus.O);
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomFull", "Room is already full");
            }
        }

        public async Task Click(int pos)
        {
            if (participants.Count < maxParticipants)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomFull", "Room is not ready");
                return;
            }

            if ((currentTurn == EStatus.X && participants.IndexOf(Context.ConnectionId) == 0) || (currentTurn == EStatus.O && participants.IndexOf(Context.ConnectionId) == 1))
            {
                await Clients.All.SendAsync("MoveToPosition", pos, currentTurn);
                currentTurn = currentTurn == EStatus.X ? EStatus.O : EStatus.X;
                await Clients.All.SendAsync("ChangeTurn", (int)currentTurn);
            }
        }

        //public async Task ChangeTurn()
        //{

        //}

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            participants.Remove(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ChatRoom");
            await Clients.OthersInGroup("ChatRoom").SendAsync("UserLeft", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
    public enum EStatus
    {
        none,
        X,
        O
    }
}
