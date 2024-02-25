using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Core.Automations;
using NetEntityAutomation.Core.Configs;

namespace NetEntityAutomation.Core.RoomManager;

/// <summary>
/// This class is responsible for managing all automation in a room.
/// </summary>
public class Room
{
    private readonly IHaContext _haContext;
    private readonly IRoomConfig _roomConfig;
    private readonly List<IAutomationBase> _automations = [];

    public Room(IRoomConfig roomConfig, IHaContext haContext)
    {
        _roomConfig = roomConfig;
        _roomConfig.Logger.LogDebug("Creating room {RoomName}", roomConfig.Name);
        _haContext = haContext;
        InitAutomations();
    }

    private void InitAutomations()
    {
        _roomConfig.Logger.LogDebug("Creating automations");
        foreach (var automation in _roomConfig.Entities)
        {
            _roomConfig.Logger.LogDebug("Created {AutomationType}", automation.AutomationType);
            switch (automation.AutomationType)
            {
                case AutomationType.MainLight:
                    _automations.Add(new MainLightAutomationBase(_haContext, automation, _roomConfig.Logger));
                    break;
                case AutomationType.SecondaryLight:
                    _automations.Add(new LightAutomationBase(_haContext, automation, _roomConfig.Logger));
                    break;
                case AutomationType.Blinds:
                    _automations.Add(new BlindAutomationBase(_haContext, automation, _roomConfig.Logger));
                    break;
                default:
                    break;
            }
        }

        _roomConfig.Logger.LogDebug("Number of automations: {AutomationCount}", _automations.Count);
    }
}