using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Core.Configs;

namespace NetEntityAutomation.Core.Automations;

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

public abstract class AutomationBase
{
    protected IHaContext Context { get; set; }
    protected ILogger Logger { get; set; }
    protected AutomationConfig Config { get; set; }
    protected IObservable<bool> IsEnabledObserver;
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
    protected AutomationBase(IHaContext context, ILogger logger, AutomationConfig config)
    {
        Context = context;
        Logger = logger;
        Config = config;
        IsEnabledObserver = Observable.FromEventPattern<bool>(
            handler => IsEnabledChanged += handler,
            handler => IsEnabledChanged -= handler
        ).Select(pattern => pattern.EventArgs);
        // InitServices();
    }

    protected void DailyEventAtTime(TimeSpan timeSpan, Action action)
    {
        var triggerIn = timeSpan - DateTime.Now.TimeOfDay;
        Observable.Timer(triggerIn, TimeSpan.FromDays(1)).Subscribe(e =>
        {
            Logger.LogDebug("Daily event at {Time} triggered", timeSpan);
            action();
        });
        Logger.LogDebug("Triggering first event in {Time}", triggerIn);
    }
    
    // private void InitServices()
    // {
    //     Context.RegisterServiceCallBack<ServiceData>($"automation_{Config.Name.Replace(' ', '_').ToLower()}_service", 
    //         e =>
    //         {
    //             if (Enum.TryParse<ServiceAction>(e.action, ignoreCase: true, out var action))
    //             {
    //                 
    //                 Logger.LogInformation("Service called action: {Action}", action);
    //                 IsEnabled = action switch
    //                 {
    //                     ServiceAction.Disable => false,
    //                     ServiceAction.Enable => true,
    //                     ServiceAction.Toggle => !isEnabled,
    //                     _ => isEnabled
    //                 };
    //                 
    //                 Logger.LogDebug("Automation {AutomationName} is now {AutomationState}", Config.Name, isEnabled ? "enabled" : "disabled");
    //             }
    //             else
    //             {
    //                 Logger.LogWarning("Service called with unknown action: {Action} value: {value}",
    //                     e.action, e.value);
    //             }
    //         });
    // }
}