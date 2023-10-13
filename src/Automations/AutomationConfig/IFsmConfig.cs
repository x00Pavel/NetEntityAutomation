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
    public INightModeConfig NightMode { get; init; }

    /// <summary> Function that dynamically returns the start time </summary>
    public Func<TimeSpan> StartAtTimeFunc { get; }

    /// <summary> Function that dynamically returns the stop time </summary>
    public Func<TimeSpan> StopAtTimeFunc { get; }

    public bool IsWorkingHours { get; }
    public double Transition { get; set; }
    
    
    
    public IEnumerable<Func<bool>> SensorConditions { get; init; }
    
    public IEnumerable<Func<bool>> SwitchConditions { get; init; }
    
    public bool SensorConditionMet { get; }
    
    public bool SwitchConditionMet { get; }
}