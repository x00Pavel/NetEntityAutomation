using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public record LightAutomationConfiguration<TFsmState>: IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; set; }
    public IEnumerable<IBinarySensorEntityCore> MotionSensors { get; set; }
    public IEnumerable<string>? SwitchIds { get; set; }
    public IFsmConfig<TFsmState> FsmConfig { get; set; }
    
}