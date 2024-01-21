using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; }
    public string ProgramName => Name.ToLower().Replace(' ', '_');
    public IEnumerable<IBinarySensorEntityCore> MotionSensors { get; }
    public IEnumerable<string>? SwitchIds { get; }
    public IFsmConfig<TFsmState> FsmConfig { get; }
}