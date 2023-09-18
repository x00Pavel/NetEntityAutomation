using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;


public interface IFsmConfig<TFsmState> where TFsmState : Enum
{
    public IEnumerable<ILightEntityCore> Lights { get; }
    public TimeSpan HoldOnTime { get; }
    public TimeSpan WaitForOffTime { get; }
    
    public long HoldOnTimeSeconds { get; init; }
    public long HoldOnTimeMinutes { get; init; }
    public long WaitForOffSeconds { get; init; }
    public long WaitForOffMinutes { get; init; }

    public TFsmState InitialState { get; }

    /// <summary> Add custom behaviour during the night </summary>
    public bool NightMode { get; init; }

    /// <summary> Function that dynamically returns the start time </summary>
    public Func<TimeSpan> StartAtTimeFunc { get; }

    /// <summary> Function that dynamically returns the stop time </summary>
    public Func<TimeSpan> StopAtTimeFunc { get; }

    public bool IsWorkingHours { get; }
    long? NightModeBrightness { get; set; }
    long? Transition { get; set; }
}

public interface IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; }
    public IBinarySensorEntityCore MotionSensors { get; }
    public string? SwitchId { get; }
    public IFsmConfig<TFsmState> FsmConfig { get; }
}

public interface IRoom
{
    public string Name { get; }
}

public interface IRoomConfig<TFsmState>: IRoom where TFsmState : Enum
{
    public IAutomationConfig<TFsmState> AutomationConfig { get; }
}