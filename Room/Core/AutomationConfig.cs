using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;
using NetEntityAutomation.Room.Conditions;
using NetEntityAutomation.Room.Interfaces;
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
    public IEnumerable<MotionSensor> Triggers {get ; set;} = new List<MotionSensor>();
    public IEnumerable<ICondition> Conditions { get; set; } = new List<ICondition> {new DefaultCondition()};
    public string ServiceAccountId { get; set; }
    public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan SwitchTimer { get; set; } = TimeSpan.FromHours(2);
    public Func<TimeSpan> StopAtTimeFunc { get; init; } = () => DateTime.Parse("16:00:00").TimeOfDay;
    public Func<TimeSpan> StartAtTimeFunc { get; init; } = () => DateTime.Parse("06:30:00").TimeOfDay;
    public NightModeConfig NightMode { get; set; } = new();
    
}