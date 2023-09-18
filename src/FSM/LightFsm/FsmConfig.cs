using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.FSM.LightFsm;

public record NightModeConfig: INightModeConfig
{
    public bool IsEnabled { get; set; } = false;
    public List<ILightEntityCore>? Devices { get; init; }
    public long? NightModeBrightness { get; set; } = 40;
    public long? Transition { get; set; } = 2;
    public Func<TimeSpan> StopAtTimeFunc { get; init; } = () => DateTime.Parse("05:00:00").TimeOfDay;
    public Func<TimeSpan> StartAtTimeFunc { get; init; } = () => DateTime.Parse("23:30:00").TimeOfDay;
    
    public bool IsWorkingHours { get
    {
        var now = DateTime.Now.TimeOfDay;
        return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
    } }
}

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

    /// <summary> Add custom behaviour during the night </summary>
    public INightModeConfig NightMode { get; init; } = new NightModeConfig();

    public required IEnumerable<ILightEntityCore> Lights { get; init; }

    /// <summary> Function that dynamically returns the start time </summary>
    public Func<TimeSpan> StartAtTimeFunc { get; set; } = () => DateTime.Parse(DefaultStartTime).TimeOfDay;

    /// <summary> Function that dynamically returns the stop time </summary>
    public Func<TimeSpan> StopAtTimeFunc { get; set; } = () => DateTime.Parse(DefaultStopTime).TimeOfDay;

    public bool IsWorkingHours
    {
        get
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
        }
    }
}