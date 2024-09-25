using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.ViewModels;
using System.Text.RegularExpressions;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public RoomsController(ILogger<RoomsController> logger, IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
            _logger = logger;
        }

        [HttpGet("create")]
        public IActionResult CreateRooms(int roomCapacity)
        {
            if (RoomManager.Rooms.Count() == 0)  // Initialize only if rooms are not already created
            {
                for (int i = 1; i <= roomCapacity; i++)
                {
                    RoomManager.Rooms.Add(i, new RoomsVM() { RoomId = i});  // Create empty rooms with IDs from 1 to roomCount
                }
                _logger.LogInformation($"{roomCapacity} rooms created.");
                return Ok($"{roomCapacity} rooms created.");
            }
            else
            {
                return BadRequest("Rooms are already created.");
            }
        }


        //[HttpGet]
        //public IActionResult GetRooms()
        //{
        //    var roomNames = rooms.Keys.Select(r => $"Room-{r}").ToList();
        //    return Ok(roomNames);  // Returns the list of room names like Room-1, Room-2, etc.
        //}


        // POST: api/Rooms/join/1
        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromBody] string connectionId)
        {
            if (!RoomManager.Rooms.ContainsKey(roomId))
            {
                return NotFound("Room does not exist.");
            }

            var room = RoomManager.Rooms[roomId];

            // Check if the room is already full
            if (room.IsRoomFull)
            {
                return BadRequest("Room is full.");
            }

            await _chatHubContext.Groups.AddToGroupAsync(connectionId, $"Room-{roomId}");

            // Add the participant to the room
            room.Participants.Add(connectionId);
            await _chatHubContext.Clients.Client(connectionId).SendAsync("NotiStatus", room.Participants.Count == 1 ? EStatus.X : EStatus.O);
            // If room has enough participants, start the game
            if (room.IsRoomFull)
            {
                room.IsGameStarted = true;
                // Notify the participants that the game is starting
                await _chatHubContext.Clients.Group($"Room-{roomId}")
                    .SendAsync("GameStarted", roomId);
            }

            // Notify users in the room via SignalR
            await _chatHubContext.Clients.Group($"Room-{roomId}")
                .SendAsync("UserJoined", connectionId, roomId);

            return Ok($"Connection {connectionId} joined Room-{roomId}.");
        }
    }

    //public class Room
    //{
    //    public int RoomId { get; set; }  // Room Identifier
    //    public List<string> Participants { get; set; } = new List<string>();  // List of participants (ConnectionIds)
    //    public bool IsGameStarted { get; set; }  // Indicates if the game has started

    //    public int MaxParticipants { get; set; } = 2;  // Max number of participants (default to 2)

    //    // Property to check if the room is ready to start (when there are enough participants)
    //    public bool IsRoomFull
    //    {
    //        get
    //        {
    //            return Participants.Count >= MaxParticipants;
    //        }
    //    }
    //}

}
