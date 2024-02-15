using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Core.Configs;

/// <summary>
/// Night mode configuration.
/// <remarks>
/// Currently, it is used mostly for light automations.
/// </remarks>
/// </summary>
public record NightModeConfig
{
    /// <summary>
    /// Enable night mode for the automation.
    /// </summary>
    public bool IsEnabled { get; set; } = false;
    /// <summary>
    /// A list of devices that will be used in the night mode.
    /// <remarks>
    /// Currently, it is used on;y for light automations.
    /// </remarks>
    /// </summary>
    public List<ILightEntityCore>? Devices { get; init; }
    /// <summary>
    /// Light parameters for the night mode to be set on specific devices.
    /// <para>
    /// Default value is 40% brightness and 2 seconds transition.
    /// </para>
    /// <remarks>
    /// Currently, it is used on;y for light automations.
    /// </remarks>
    /// </summary>
    public LightParameters LightParameters { get; init; } = new()
    {
        BrightnessPct = 40,
        Transition = 2
    };
    
    /// <summary>
    /// Callable to specify the time when the night mode should stop.
    /// <para>
    /// Default value is 05:00:00.
    /// </para>
    /// <remarks>
    /// The callable is used to set dynamic time for the night mode to stop.
    /// For example, relative to the next sunrise or sunset.
    /// </remarks>
    /// </summary>
    public Func<TimeSpan> StopAtTimeFunc { get; init; } = () => DateTime.Parse("05:00:00").TimeOfDay;
    /// <summary>
    /// Callable to specify the time when the night mode should start.
    /// <para>
    /// Default value is 23:30:00.
    /// </para>
    /// <remarks>
    /// <para>
    /// The callable is used to set dynamic time for the night mode to stop.
    /// For example, relative to the next sunrise or sunset.
    /// </para>
    /// </remarks>
    /// </summary>
    public Func<TimeSpan> StartAtTimeFunc { get; init; } = () => DateTime.Parse("23:30:00").TimeOfDay;
    
    /// <summary>
    /// Helper method to check if the current time is within the night mode time.
    /// Used in automation code.
    /// </summary>
    public bool IsWorkingHours
    {
        get
        {
            var now = DateTime.Now.TimeOfDay;
            return now >= StartAtTimeFunc() || now <= StopAtTimeFunc();
        }
    }
}