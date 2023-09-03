using NetDaemon.HassModel.Entities;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public record LightAutomationConfiguration<TFsmState>(string Name, IBinarySensorEntityCore Sensor, string SwitchId,
    FsmConfig<TFsmState> FsmConfig) where TFsmState : Enum
{
    public string? Name { get; init; } = Name;
    public IBinarySensorEntityCore MotionSensors { get; init; } = Sensor;
    public string Switch { get; init; } = SwitchId;
    public FsmConfig<TFsmState> FsmConfig { get; set; } = FsmConfig;
}