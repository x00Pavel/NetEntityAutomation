using Microsoft.Extensions.Logging;

namespace NetEntityAutomation.Automations.AutomationConfig;

/// <summary>
/// Empty interface to allow configuration classes to be added to DI container
/// </summary>
public interface IRoom;

public interface IRoomConfig<TFsmState>: IRoom where TFsmState : Enum
{
    public string Name { get; }
    public ILogger Logger { get; }
    public ILightAutomationConfiguration<TFsmState> AutomationConfig { get; }
}