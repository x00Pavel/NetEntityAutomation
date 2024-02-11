using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Room.Interfaces;

namespace NetEntityAutomation.Room.Core;
/// <summary>
/// This class is responsible for managing all automation in a room.
/// </summary>
public class Room
{   
    private readonly IHaContext _haContext;
    private readonly IRoomConfigV1 _roomConfig;
    private IEnumerable<IAutomation> _automations;
    public Room(IRoomConfigV1 roomConfig, IHaContext haContext)
    {   
        _roomConfig = roomConfig;
        _roomConfig.Logger.LogDebug("Creating room {RoomName}", roomConfig.Name);
        _haContext = haContext;
        _automations = new List<IAutomation>();
        InitAutomations();
    }

    private void InitAutomations()
    {   
        _roomConfig.Logger.LogDebug("Creating automations");
        foreach (var automation in _roomConfig.Entities)
        {   
            switch (automation.AutomationType)
            {
                case AutomationType.MainLight:
                    break;
                case AutomationType.SecondaryLight:
                    _automations = _automations.Append(new LightAutomation(_haContext, automation, _roomConfig.Logger));
                    _roomConfig.Logger.LogDebug("Created SecondaryLight");
                    break;
                case AutomationType.Blinds:
                    break;
                default:
                    break;
            }
        }
        
        _roomConfig.Logger.LogDebug("Number of automations: {AutomationCount}", _automations.Count());
    }
}