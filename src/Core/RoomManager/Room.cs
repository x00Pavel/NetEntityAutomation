using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Integration;
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
        InitServices();
    }

    private void InitServices()
    {
        var serviceName = _roomConfig.Name.ToLower().Replace(' ', '_') + "_service";
        _haContext.RegisterServiceCallBack<RoomData>("toggle_" + serviceName, data =>
        {   
            _automations.ForEach(auto =>
            {
                auto.IsEnabled = !auto.IsEnabled;
                _roomConfig.Logger.LogDebug("Toggling automation {AutomationName} to {IsEnabled}", 
                    auto.GetType().Name,
                    auto.IsEnabled ? "enabled" : "disabled");
            });
        });
        _roomConfig.Logger.LogDebug("Service {ServiceName} initialised", serviceName);
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

internal record RoomData
{
}