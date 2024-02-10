using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Events;
using NetEntityAutomation.FSM;


namespace NetEntityAutomation.Room;

public record RoomConfiguration
{
    public string? Name { get; init; }
    public BinarySensorEntity[]? MotionSensors { get; init; }
    public LightSwitchPair[]? Lights { get; init; }
}

public record LightSwitchPair
{
    public string? Switch { get; init; }
    public LightEntity[]? Light { get; init; }
}

public interface IRoom
{
    public string Name { get; }
    public RoomConfiguration Configuration { get; init; }
    public IEnumerable<BinarySensorEntity> MotionSensors { get; }
    public IEnumerable<LightSwitchPair> Lights { get;  }
}

public abstract class Room: IRoom
{
    protected IHaContext Ha { init; get; }
    public RoomConfiguration Configuration { get; init; }
    public string Name => Configuration.Name;
    public IEnumerable<BinarySensorEntity> MotionSensors => Configuration.MotionSensors ?? Array.Empty<BinarySensorEntity>();
    public IEnumerable<LightSwitchPair> Lights => Configuration.Lights;
    protected IEnumerable<LightEntity> AllLights => Lights.SelectMany(pair => pair.Light!);
    protected IObservable<StateChange> MotionSensorOn => MotionSensors.StateChanges().Where(e => e.New?.State == "on");
    protected IObservable<StateChange> MotionSensorOff => MotionSensors.StateChanges().Where(e => e.New?.State == "off");

    protected IObservable<ZhaEventData>? SwitchEvent { get; set; }

    protected IObservable<ZhaEventData> SwitchEventOn => SwitchEvent.Where( e => e.Command == "on");
    protected IObservable<ZhaEventData> SwitchEventOff => SwitchEvent.Where( e => e.Command == "off");
    protected IEntities _entities { get; set; }
    protected ILogger<Room> Logger { init; get; }
    protected MotionSwitchLightFSM _fsm { get; set; }
    
    protected Room(IHaContext ha, IOptions<RoomConfiguration> config, ILogger<Room> logger, IScheduler scheduler)
    {   
        Configuration = config.Value;
        Logger = logger;
        Ha = ha;
        _entities = new Entities(ha);
        _fsm = new MotionSwitchLightFSM(Logger);

        ha.StateChanges()
            .Where(e => e.New?.EntityId == "input_button.trigger_time_elapsed")
            .Subscribe(_ => _fsm.TimeElapsed());
        
        ha.StateChanges()
            .Where(e=> e.New?.EntityId == "input_button.trigger_motion_on")
            .Subscribe(_=> _fsm.MotionOn());
        
        ha.StateChanges()
            .Where(e => e.New?.EntityId == "input_button.trigger_motion_off")
            .Subscribe(_ => _fsm.MotionOff());
        
        ha.StateChanges()
            .Where(e=> e.New?.EntityId == "input_button.trigger_switch_on")
            .Subscribe(_=> _fsm.SwitchOn());
        
        ha.StateChanges()
            .Where(e=> e.New?.EntityId == "input_button.trigger_switch_off")
            .Subscribe(_=> _fsm.SwitchOff());
        
        
        if (InitSwitches())
        {
            foreach (var pair in Lights)
            {
                SwitchEventOn.Subscribe(_=> pair.Light?.TurnOn(transition: 2, brightnessPct: 100));
                SwitchEventOff.Subscribe(_ => pair.Light?.TurnOff(transition: 2));
                foreach (var l in pair.Light)
                {
                    new ZigbeeSwitch(SwitchEvent!, l, scheduler);
                }
            }
        }
        
        // There is no need to check for null values as there is a default empty list in the property
        MotionSensorOn.Subscribe(_ => TurnOnALlLights());
        MotionSensorOff.Subscribe(_ => TurnOffAllLights());
    }
    
    protected void TurnOffAllLights()
    {
        Logger.LogInformation("Turning off all lights in {Name}", Name);
        AllLights.TurnOff(transition:2);
    }

    protected void TurnOnALlLights()
    {
        var sun = _entities.Sun.Sun;
        Logger.LogInformation("Turning on all lights in {Name}", Name);
        var nextDusk = DateTime.Parse(sun.EntityState?.Attributes?.NextDusk!);
        if (nextDusk.Subtract(DateTime.Now).TotalHours < 1)
        {   
            AllLights.TurnOn(transition:2, brightnessPct: 100);
        }
        else if (sun.EntityState?.State == "below_horizon")
        {
            AllLights.TurnOn(transition: 3, brightnessPct: 40);
        }
    }

    private bool InitSwitches()
    {
        if (Lights.Any(p => p.Switch == null)) return false;
        
        SwitchEvent = Ha.Events.Filter<ZhaEventData>("zha_event")
            // This will triggered by any switch in the room
            .Where(e => Lights.Any(p => p.Switch == e.Data?.DeviceIeee))
            .Select(e => e.Data!);
        return true;
    }
}