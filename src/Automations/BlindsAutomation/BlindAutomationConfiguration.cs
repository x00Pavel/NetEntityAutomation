using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.Automations.BlindsAutomation;

public interface IBlindAutomationConfig: IAutomationConfig
{
    public ICoverEntityCore Blind { get; }
}

public record BlindAutomationConfiguration: IBlindAutomationConfig
{
    public ICoverEntityCore Blind { get; }
}