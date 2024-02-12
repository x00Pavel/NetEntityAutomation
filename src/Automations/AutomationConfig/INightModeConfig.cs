using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface INightModeConfig
{
    public bool IsEnabled { get; set; }
    public List<ILightEntityCore>? Devices { get; init; }
    public LightParameters LightParameters { get; init; }
    public Func<TimeSpan> StopAtTimeFunc { get; }
    public Func<TimeSpan> StartAtTimeFunc { get; }
    
    public bool IsWorkingHours { get; }
}