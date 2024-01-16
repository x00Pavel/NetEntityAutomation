using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Integration;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.Automations.LightAutomation;



// dictionary with maping of FSM state to automation class
// static Dictionary<string, string> fsmToAutomationMap = new Dictionary<string, string>();

internal enum ServiceAction
{
    Disable,
    Enable,
    Toggle,
}

record ServiceData
{
    public string? action { get; init; }
    public string? value { get; init; }
}

public abstract class BaseAutomation<TFsmState> where TFsmState: struct, Enum
{
    protected ILogger Logger;

    protected IAutomationConfig<TFsmState> Config;

    protected IHaContext HaContext;

    private bool IsEnabled { get; set; } = true;
    
    protected BaseAutomation(
        ILogger logger,
        IAutomationConfig<TFsmState> config,
        IHaContext haContext
        )
    {
        Logger = logger;
        Config = config;
        HaContext = haContext;
        InitServices();
    }
    
    private void InitServices()
    {
        HaContext.RegisterServiceCallBack<ServiceData>($"disable_automation_{Config.Name.Replace(' ', '_').ToLower()}", 
            e =>
            {
                if (Enum.TryParse<ServiceAction>(e.action, ignoreCase: true, out var action))
                {
                    
                    Logger.LogInformation("Service called action: {Action}", action);
                    IsEnabled = action switch
                    {
                        ServiceAction.Disable => false,
                        ServiceAction.Enable => true,
                        ServiceAction.Toggle => !IsEnabled,
                        _ => IsEnabled
                    };
                    Logger.LogDebug("Automation {AutomationName} is now {AutomationState}", Config.Name, IsEnabled ? "enabled" : "disabled");
                }
                else
                {
                    Logger.LogWarning("Service called with unknown action: {Action} value: {value}",
                        e.action, e.value);
                }
            });

    }

    protected abstract void InitFsmTransitions();
}