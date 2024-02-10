using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;

namespace NetEntityAutomation.Room.Core;
/// <summary>
/// This class is responsible for managing all the rooms.
/// It is used as a scoped service and initiated on program start.
/// Main purpose of this service is to create all the rooms.
/// </summary>
public class RoomManager: IRoomManager
{   
    private readonly ILogger _logger;
    private readonly List<Room> _rooms;
 
    public RoomManager(IHaContext haContext, ILogger logger, IEnumerable<IRoomConfigV1> rooms)
    {
        _logger = logger;
        _rooms = new List<Room>();
        _logger.LogInformation("Initialising room manager");
        _logger.LogInformation("Number of rooms: {RoomCount}", rooms.Count());
        if (haContext == null)
        {
            _logger.LogError("HaContext is null");
            throw new ArgumentNullException(nameof(haContext));
        }
        
        foreach (var roomConfig in rooms)
        {   
            _rooms.Add(new Room(roomConfig, haContext));
        }
    }
}