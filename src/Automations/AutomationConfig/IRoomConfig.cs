using Microsoft.Extensions.Logging;

namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IRoom
{
    public string Name { get; }
    // public object AutomationConfig { get; }
    public ILogger Logger { get; }
}

public interface IRoomConfig<TFsmState>: IRoom where TFsmState : Enum
{
    public IAutomationConfig<TFsmState> AutomationConfig { get; }
}