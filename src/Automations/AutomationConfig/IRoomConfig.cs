namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IRoom
{
    public string Name { get; }
}

public interface IRoomConfig<TFsmState>: IRoom where TFsmState : Enum
{
    public IAutomationConfig<TFsmState> AutomationConfig { get; }
}