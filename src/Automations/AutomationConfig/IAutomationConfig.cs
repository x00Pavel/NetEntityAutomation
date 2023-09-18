using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; }
    public IBinarySensorEntityCore MotionSensors { get; }
    public string? SwitchId { get; }
    public IFsmConfig<TFsmState> FsmConfig { get; }
}