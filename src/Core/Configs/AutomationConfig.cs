using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Core.Conditions;
using NetEntityAutomation.Core.Triggers;

namespace NetEntityAutomation.Core.Configs;

/// <summary>
/// Class represents a configuration for automation.
/// It is used to configure the automation by providing entities, triggers, conditions and other settings.
/// </summary>
public interface IAutomationConfig
{
    /// <summary>
    /// Entities that are used by the automation (including secondary entities that acts as a trigger like a Sun entity).
    /// On automation configuration phase, this list will be filtered to the specific type of entities based on automation type.
    /// </summary>
    
    /// <summary>
    /// Triggers are used to trigger the automation.
    /// The automation is relies that trigger will return input_boolean meaning On/Off state.
    /// </summary>
    public IEnumerable<IStateChangeTrigger> Triggers { get; set; } //= new List<IStateChangeTrigger>();
    
    /// <summary>
    /// A list of conditions when the automation should be triggered.
    /// Working hours and night mode are not included here.
    /// It is mostly used for custom conditions.
    /// E.g if someone is at home, if tracker device is in sleep mode, etc.
    /// </summary>
    public IEnumerable<ICondition> Conditions { get; set; } //= new List<ICondition> { new DefaultCondition() };
    
    /// <summary>
    /// Account ID which owns the token for NetDaemon.
    /// This value is used to distinguish between different user input events and events triggered by automation.
    /// </summary>
    public string ServiceAccountId { get; set; }
    
    /// <summary>
    /// Time for waiting for the next trigger event.
    /// E.g. in light scenario, after light was turned on by automation (triggered by motion sensor), the automation will check for motion after this period of time.
    /// </summary>
    public TimeSpan WaitTime { get; set; } // = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// Time after manual interaction with the automation to trigger the event.
    /// E.g. in light scenario, after light was turned on by switch (manualy, from HA), the automation will turn off the light if there is no motion after this period of time.
    /// </summary>
    public TimeSpan SwitchTimer { get; set; } // = TimeSpan.FromHours(2);
    
    /// <summary>
    /// Callable that returns time when the automation should stop working.
    /// <remarks>
    /// <list type="bullet">
    ///     <item>
    ///         For lights automation this time is a after which the lights should be turned off
    ///     </item>
    ///     <item>
    ///         For blinds automation this is a time after which the blinds should be opened.
    ///         If not set (null value), then sun position is used to for automation.
    ///     </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public Func<TimeSpan>? StopAtTimeFunc { get; set; } //= () => DateTime.Parse("06:00").TimeOfDay;
    
    /// <summary>
    /// Callable that returns time when the automation should start working.
    /// <remarks>
    /// <list type="bullet">
    ///     <item>
    ///         For lights automation this time is a after which the lights should be turned on
    ///     </item>
    ///     <item>
    ///         For blinds automation this is a time after which the blinds should be closed.
    ///         If not set (null value), then sun position is used to for automation.
    ///     </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public Func<TimeSpan>? StartAtTimeFunc { get; set; } //= () => DateTime.Parse("18:00").TimeOfDay;
    
    /// <summary>
    /// Configuration for night mode. 
    /// </summary>
    public NightModeConfig NightMode { get; set; } //= new();
    
    public IHaContext Context { get; set; }
    
    public ILogger Logger { get; set; }
    
    public void ConfigureAutomation();
}