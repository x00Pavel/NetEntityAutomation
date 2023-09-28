namespace NetEntityAutomation.Automations.AutomationConfig;

public interface IRoom
{
    public string Name { get; }
    public IAutomationConfig AutomationConfig { get; }
}
