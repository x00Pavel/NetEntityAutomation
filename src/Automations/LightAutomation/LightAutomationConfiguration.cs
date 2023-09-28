using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public interface ILightAutomationConfig<TFsmState>: IAutomationConfig where TFsmState : Enum
{
    public string Name { get; }
    public string JsomConfigPath { get; }
    public IBinarySensorEntityCore MotionSensors { get; }
    public string? SwitchId { get; }
    public IFsmConfig<TFsmState> FsmConfig { get; }
}

public record LightAutomationConfiguration<TFsmState>: ILightAutomationConfig<TFsmState> where TFsmState : Enum      
{
    public string Name { get; set; }
    public string JsomConfigPath { get; set; } = string.Empty;
    public IBinarySensorEntityCore MotionSensors { get; set; }
    public string? SwitchId { get; set;  }
    public IFsmConfig<TFsmState> FsmConfig { get; set; }
    
}