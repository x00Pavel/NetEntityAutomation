using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.LightExtensionMethods;

namespace NetEntityAutomation.FSM.LightFsm;

public record NightModeConfig: INightModeConfig
{
    public bool IsEnabled { get; set; } = false;
    public List<ILightEntityCore>? Devices { get; init; }
    public LightParameters LightParameters { get; init; } = new()
    {
        BrightnessPct = 40,
        Transition = 2
    };
    public Func<TimeSpan> StopAtTimeFunc { get; init; } = () => DateTime.Parse("05:00:00").TimeOfDay;
    public Func<TimeSpan> StartAtTimeFunc { get; init; } = () => DateTime.Parse("23:30:00").TimeOfDay;
    
    public bool IsWorkingHours { get
    {
        var now = DateTime.Now.TimeOfDay;
        return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
    } }
}