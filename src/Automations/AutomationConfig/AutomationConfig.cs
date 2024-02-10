using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string? Name { get; init; }
    // used for storing name in program-friendly format (without spaces)
    public string ProgramName => Name.ToLower().Replace(' ', '_');
    public IEnumerable<IBinarySensorEntityCore> MotionSensors { get; init; }
    public IEnumerable<string>? SwitchIds { get; init;  }
    public IFsmConfig<TFsmState> FsmConfig { get; init; }
    public INightModeConfig? NightMode { get; init; }
}