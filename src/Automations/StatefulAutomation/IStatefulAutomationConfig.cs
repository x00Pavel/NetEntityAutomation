using NetEntityAutomation.Automations.AutomationConfig;

namespace StatefulAutomation;

public interface IStatefulAutomationConfig<TFsmState>: IAutomationConfig where TFsmState : Enum
{
    public IFsmConfig<TFsmState> FsmConfig { get; init; }
}