using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Integration;
using NetEntityAutomation.Core.Automations;

namespace NetEntityAutomation.Core.RoomManager;

/// <summary>
/// This class is responsible for managing all automation in a room.
/// </summary>
public class Room
{
    private readonly IHaContext _haContext;
    private readonly IRoomConfig _roomConfig;
    private readonly List<AutomationBase> _automations = [];

    public Room(IRoomConfig roomConfig, IHaContext haContext)
    {
        _roomConfig = roomConfig;
        _roomConfig.Logger.LogDebug("Creating room {RoomName}", roomConfig.Name);
        _haContext = haContext;
        InitAutomations();
        InitServices();
        // InitSensors();
    }

    private void InitServices()
    {
        var serviceName = _roomConfig.Name.ToLower().Replace(' ', '_') + "_service";
        _haContext.RegisterServiceCallBack<RoomData>("toggle_" + serviceName, data =>
        {
            if (data.Automation != null)
            {
                var automationClass = GetType().Assembly.GetTypes().First(t => t.Name == data.Automation);
                var automation = _automations.First(a => a.GetType() == automationClass);
                automation.IsEnabled = !automation.IsEnabled;
                _roomConfig.Logger.LogDebug("Toggling automation {AutomationName} to {IsEnabled}", 
                    automation.GetType().Name,
                    automation.IsEnabled ? "enabled" : "disabled");
            }
            else
            {
                _automations.ForEach(auto =>
                {
                    auto.IsEnabled = !auto.IsEnabled;
                    _roomConfig.Logger.LogDebug("Toggling automation {AutomationName} to {IsEnabled}", 
                        auto.GetType().Name,
                        auto.IsEnabled ? "enabled" : "disabled");
                });     
            }
           
        });
        _roomConfig.Logger.LogDebug("Service {ServiceName} initialised", serviceName);
    }

    private void InitAutomations()
    {
        _roomConfig.Logger.LogDebug("Creating automations");
        foreach (var automation in _roomConfig.Entities)
        { 
            _roomConfig.Logger.LogDebug("Creating {AutomationType}", automation.GetType().Name);
            automation.Context = _haContext;
            automation.Logger = _roomConfig.Logger;
            automation.ConfigureAutomation();
            _automations.Add(automation);
        }
        _roomConfig.Logger.LogDebug("Number of automations: {AutomationCount}", _automations.Count);
    }
}

internal record RoomData
{
    public string? Automation { get; set; }
}