using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using StatefulAutomation;

namespace NetEntityAutomation.Automations.LightAutomation;

public interface ILightAutomationConfig<TFsmState>: IStatefulAutomationConfig<TFsmState> where TFsmState : Enum
{
    public string Name { get; }
    public string JsomConfigPath { get; }
    public IBinarySensorEntityCore MotionSensors { get; }
    public string? SwitchId { get; }
}

