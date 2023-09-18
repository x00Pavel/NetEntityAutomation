using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface INightModeConfig
{
    public bool IsEnabled { get; set; }
    public List<ILightEntityCore>? Devices { get; init; }
    long? NightModeBrightness { get; set; }
    long? Transition { get; set; }
    public Func<TimeSpan> StopAtTimeFunc { get; }
    public Func<TimeSpan> StartAtTimeFunc { get; }
    
    public bool IsWorkingHours { get; }
}