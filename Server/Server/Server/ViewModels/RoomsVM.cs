namespace Server.ViewModels
{
    public class RoomsVM
    {
        public int RoomId { get; set; }  // Room ID
        public List<string> Participants { get; set; } = new List<string>();  // Participants (ConnectionIds)
        public bool IsGameStarted { get; set; }  // Whether the game has started
        public int MaxParticipants { get; set; } = 2;  // Maximum number of participants

        public EStatus currentTurn = EStatus.X;

        // Check if the room is full
        public bool IsRoomFull => Participants.Count >= MaxParticipants;
    }

    public static class RoomManager
    {
        public static Dictionary<int, RoomsVM> Rooms { get; set; } = new Dictionary<int, RoomsVM>();
    }

}
