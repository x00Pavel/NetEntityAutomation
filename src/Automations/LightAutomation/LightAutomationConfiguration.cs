using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public record LightAutomationConfiguration<TFsmState>: IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; set; }
    public IBinarySensorEntityCore MotionSensors { get; set; }
    public string? SwitchId { get; set;  }
    public IFsmConfig<TFsmState> FsmConfig { get; set; }
    
}