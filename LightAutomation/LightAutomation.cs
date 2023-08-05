using System.Reactive.Linq;
using FSM;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Events;

namespace LightAutomation;

public record LightAutomationConfiguration(string Name, List<ILightEntity> LightEntities, IBinarySensorEntity Sensor, string SwitchId)
{
    public string? Name { get; init; } = Name;
    public IEnumerable<ILightEntity> Lights { get; init; } = LightEntities;
    public IBinarySensorEntity MotionSensors { get; init; } = Sensor;
    public string Switch { get; init; } = SwitchId;
}

public class LightAutomation
{   
    private IHaContext _haContext;
    private readonly ILogger _logger;
    private readonly LightAutomationConfiguration _config;
    private MotionSwitchLightFsm _fsm;
    private IObservable<StateChange> MotionSensorEvent =>
        _haContext.StateChanges().Where(e => e.New?.EntityId == _config.MotionSensors.EntityId);
    private IObservable<ZhaEventData> SwitchEvent =>
        _haContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => e.Data?.DeviceIeee == _config.SwitchId)
            .Select(e => e.Data!);

    public LightAutomation(IHaContext ha, LightAutomationConfiguration config, ILogger logger)
    {
        _haContext = ha;
        _logger = logger;
        _config = config;
        var fsmConfig = new FsmConfig
        {
            Lights = config.Lights,
        };
        
        _fsm = new MotionSwitchLightFsm(logger, fsmConfig);
        InitFsmTransitions();
        logger.LogInformation("LightAutomation initialised");
    }

    private void InitFsmTransitions()
    {   
        _logger.LogInformation("Initialising FSM for {RoomName}", _config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ => _fsm.MotionOn());
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ => _fsm.MotionOff());
        SwitchEvent.Where(e => e.Command == "on").Subscribe(_ => _fsm.SwitchOn());
        SwitchEvent.Where(e => e.Command == "off").Subscribe(_ => _fsm.SwitchOff());
    }
}