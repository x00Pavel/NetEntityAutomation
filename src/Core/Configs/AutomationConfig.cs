using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Triggers;
using NetEntityAutomation.Core.Conditions;

namespace NetEntityAutomation.Core.Configs;

/// <summary>
/// Class represents a configuration for automation.
/// It is used to configure the automation by providing entities, triggers, conditions and other settings.
/// </summary>
public record AutomationConfig
{
    /// <summary>
    /// Entities that are controlled by the automation.
    /// </summary>
    public IEnumerable<IEntityCore> Entities { get; set; }
    
    /// <summary>
    /// Type of automation.
    /// All types are presented at <see cref="AutomationType">this</see> doc.
    /// </summary>
    public AutomationType AutomationType { get; set; }

    /// <summary>
    /// Triggers are used to trigger the automation.
    /// The automation is relies that trigger will return input_boolean meaning On/Off state.
    /// </summary>
    public IEnumerable<IStateChangeTrigger> Triggers { get; set; } = new List<IStateChangeTrigger>();
    public IEnumerable<ICondition> Conditions { get; set; } = new List<ICondition> { new DefaultCondition() };
    public string ServiceAccountId { get; set; }
    public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan SwitchTimer { get; set; } = TimeSpan.FromHours(2);
    public Func<TimeSpan>? StopAtTimeFunc { get; set; } = () => DateTime.Parse("06:00").TimeOfDay;
    public Func<TimeSpan>? StartAtTimeFunc { get; set; } = () => DateTime.Parse("18:00").TimeOfDay;
    public NightModeConfig NightMode { get; set; } = new();
}