using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Room.Core;

namespace NetEntityAutomation.Room.RoomManager;

/// <summary>
/// This class is responsible for managing all the rooms.
/// It is used as a scoped service and initiated on program start.
/// Main purpose of this service is to create all the rooms.
/// </summary>
public class RoomManager : IRoomManager
{
    private readonly List<Room> _rooms = [];

    public RoomManager(IHaContext haContext, ILogger<RoomManager> logger, IEnumerable<IRoomConfigV1> rooms)
    {
        logger.LogInformation("Initialising room manager");
        logger.LogInformation("Number of rooms: {RoomCount}", rooms.Count());
        if (haContext == null)
        {
            logger.LogError("HaContext is null");
            throw new ArgumentNullException(nameof(haContext));
        }

        foreach (var roomConfig in rooms)
        {
            _rooms.Add(new Room(roomConfig, haContext));
        }
    }
}