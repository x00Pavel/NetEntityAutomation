using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Room.Conditions;
using NetEntityAutomation.Room.Triggers;

namespace NetEntityAutomation.Room.Core;

public record AutomationConfig
{
    public IEnumerable<IEntityCore> Entities { get; set; }
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
    public Func<TimeSpan>? StopAtTimeFunc { get; set; }
    public Func<TimeSpan>? StartAtTimeFunc { get; set; }
    public NightModeConfig NightMode { get; set; } = new();
}