using Microsoft.AspNetCore.SignalR;
using Server.Controllers;
using Server.ViewModels;

namespace Server
{
    public class ChatHub : Hub
    {
        private static int maxParticipants = 2;
        private static List<string> participants = new List<string>();
        //private static bool resetMap = false;

        //public async Task JoinChat()
        //{
        //    if (participants.Count < maxParticipants)
        //    {
        //        participants.Add(Context.ConnectionId);
        //        await Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom");
        //        await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);  // Send the ConnectionId to the client
        //        await Clients.Groups("ChatRoom").SendAsync("UserJoined", Context.ConnectionId);
        //        await Clients.Client(Context.ConnectionId).SendAsync("NotiStatus", participants.Count == 1 ? EStatus.X : EStatus.O);
        //    }
        //    else
        //    {
        //        await Clients.Client(Context.ConnectionId).SendAsync("RoomFull", "Room is already full");
        //    }
        //}

        public async Task ReceiveConnectionId()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", Context.ConnectionId);  // Send the ConnectionId to the client
        }

        //public async Task ResetMap()
        //{
        //    resetMap = true;
        //    //return;
        //}

        public async Task ResetMap(int roomId)
        {
            if (!RoomManager.Rooms.ContainsKey(roomId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomError", "Room does not exist.");
                return;
            }

            var room = RoomManager.Rooms[roomId];
            room.currentTurn = EStatus.X;  // Reset the current turn to X

            await Clients.Group($"Room-{roomId}").SendAsync("ResetGame", "The game has been reset. It's X's turn.");
            await Clients.Group($"Room-{roomId}").SendAsync("ChangeTurn", (int)room.currentTurn);
        }


        public async Task Click(int roomId, int pos)
        {
            if (!RoomManager.Rooms.ContainsKey(roomId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomError", "Room does not exist.");
                return;
            }
            var room = RoomManager.Rooms[roomId];

            if (room.Participants.Count < maxParticipants)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomFull", "Room is not ready");
                return;
            }

            if ((room.currentTurn == EStatus.X && room.Participants.IndexOf(Context.ConnectionId) == 0) || (room.currentTurn == EStatus.O && room.Participants.IndexOf(Context.ConnectionId) == 1))
            {
                await Clients.Group($"Room-{roomId}").SendAsync("MoveToPosition", pos, room.currentTurn);
                room.currentTurn = room.currentTurn == EStatus.X ? EStatus.O : EStatus.X;
                await Clients.Group($"Room-{roomId}").SendAsync("ChangeTurn", (int)room.currentTurn);
                //await Clients.Group($"Room-{roomId}").SendAsync("CheckWin", "");
            }
        }

        public async Task ChangeTurn(int roomId)
        {
            if (RoomManager.Rooms.ContainsKey(roomId))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("RoomError", "Room does not exist.");
                return;
            }
            var room = RoomManager.Rooms[roomId];
            room.currentTurn = room.currentTurn == EStatus.X ? EStatus.O : EStatus.X;
            await Clients.Group($"Room-{roomId}").SendAsync("ChangeTurn", (int)room.currentTurn);
        }

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
