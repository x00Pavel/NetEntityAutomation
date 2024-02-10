using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.FSM.LightFsm;

public class FsmConfig<TFsmState>: IFsmConfig<TFsmState> where TFsmState : Enum
{
    private const string DefaultStartTime = "19:00:00";
    private const string DefaultStopTime = "06:00:00";
    public long HoldOnTimeSeconds { get; init; } = 360;
    public long HoldOnTimeMinutes { get; init; } = 0;
    public TimeSpan HoldOnTime => TimeSpan.FromSeconds(HoldOnTimeSeconds) + TimeSpan.FromMinutes(HoldOnTimeMinutes);

    public long WaitForOffSeconds { get; init; } = 180;
    public long WaitForOffMinutes { get; init; } = 0;
    public TimeSpan WaitForOffTime => TimeSpan.FromSeconds(WaitForOffSeconds) + TimeSpan.FromMinutes(WaitForOffMinutes);

    public required TFsmState InitialState { get; init; }
    
    // Moved to LightAutomationConfig
    // public required IEnumerable<ILightEntityCore> Lights { get; init; }

    /// <summary> Function that dynamically returns the start time </summary>
    public Func<TimeSpan> StartAtTimeFunc { get; set; } = () => DateTime.Parse(DefaultStartTime).TimeOfDay;

    /// <summary> Function that dynamically returns the stop time </summary>
    public Func<TimeSpan> StopAtTimeFunc { get; set; } = () => DateTime.Parse(DefaultStopTime).TimeOfDay;

    public double Transition { get; set; } = 2.5;
    public IEnumerable<Func<bool>> SensorConditions { get; init; } = new [] { () => true };
    public IEnumerable<Func<bool>> SwitchConditions { get; init; } = new [] { () => true };
    public bool SensorConditionMet => SensorConditions.All(c => c());
    public bool SwitchConditionMet => SwitchConditions.All(c => c());

    public bool IsWorkingHours
    {
        get
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
        }
    }
    
}