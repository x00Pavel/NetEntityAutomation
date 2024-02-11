using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface ILightAutomationConfiguration<TFsmState>: IAutomationConfig<TFsmState> where TFsmState : Enum
{
    public IEnumerable<ILightEntityCore> Lights { get; }
    public double Transition { get; set; }
    public string ServiceAccountId { get; set; }
}

public class LightAutomationConfiguration<TFsmState>: ILightAutomationConfiguration<TFsmState> where TFsmState : Enum
{
    public string Name { get; init; }
    public IEnumerable<IBinarySensorEntityCore> MotionSensors { get; init; }
    public IEnumerable<string>? SwitchIds { get; init; }
    public IFsmConfig<TFsmState> FsmConfig { get; init; }
    public INightModeConfig? NightMode { get; init; }

    public IEnumerable<ILightEntityCore> Lights { get; init; }
    public double Transition { get; set; }
    public string ServiceAccountId { get; set; }
}
