using System.Reactive.Linq;
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

internal record ServiceData
{
    public string? action { get; init; }
    public string? value { get; init; }
}

public abstract class BaseAutomation<TFsmState> where TFsmState: struct, Enum
{
    protected ILogger Logger;

    protected IAutomationConfig<TFsmState> Config;

    protected IHaContext HaContext;

    protected readonly IObservable<bool> IsEnabledObserver;
    private bool isEnabled { get; set; } = true;
    public event EventHandler<bool>? IsEnabledChanged;
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value) return;
            isEnabled = value;
            IsEnabledChanged?.Invoke(this, value);
        }
    }
    
    protected BaseAutomation(
        ILogger logger,
        IAutomationConfig<TFsmState> config,
        IHaContext haContext
        )
    {
        Logger = logger;
        Config = config;
        HaContext = haContext;
        // Creates observable event for IsEnabledChanged
        IsEnabledObserver = Observable.FromEventPattern<bool>(
            handler => IsEnabledChanged += handler,
            handler => IsEnabledChanged -= handler
        ).Select(pattern => pattern.EventArgs);
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
                        ServiceAction.Toggle => !isEnabled,
                        _ => isEnabled
                    };
                    
                    Logger.LogDebug("Automation {AutomationName} is now {AutomationState}", Config.Name, isEnabled ? "enabled" : "disabled");
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