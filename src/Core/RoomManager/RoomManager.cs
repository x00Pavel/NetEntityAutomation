using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;

namespace NetEntityAutomation.Core.RoomManager;

/// <summary>
/// This class is responsible for managing all the rooms.
/// It is used as a scoped service and initiated on program start.
/// Main purpose of this service is to create all the rooms.
/// </summary>
public class RoomManager : IRoomManager
{
    private readonly List<Room> _rooms = [];
    private readonly List<IRoomConfig> configs;
    private readonly IHaContext haContext;

    public RoomManager(IHaContext context, ILogger<RoomManager> logger, IEnumerable<IRoomConfig> rooms)
    {
        logger.LogInformation("Initialising room manager");
        haContext = context ?? throw new ArgumentNullException(nameof(context));
        configs = rooms.ToList();
        configs.ForEach(config =>  _rooms.Add(new Room(config, haContext)));
        logger.LogInformation("Number of rooms: {RoomCount}", configs.Count);
    }
}