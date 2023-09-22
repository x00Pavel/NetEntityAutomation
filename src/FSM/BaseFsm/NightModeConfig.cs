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
    public Func<TimeSpan> StartAtTimeFunc { get; init; } = () => DateTime.Parse("23:00:00").TimeOfDay;
    
    public bool IsWorkingHours { get
    {
        var now = DateTime.Now.TimeOfDay;
        return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
    } }
}