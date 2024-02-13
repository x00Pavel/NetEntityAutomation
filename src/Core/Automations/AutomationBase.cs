using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Extensions.Events;

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

public interface IAutomationBase
{
}

public abstract class AutomationBase<TEntity, TFsm>: IAutomationBase
{
    protected IHaContext Context { get; set; }
    protected ILogger Logger { get; set; }
    protected AutomationConfig Config { get; set; }
    protected IEnumerable<TEntity> EntitiesList { get; set; }
    protected List<TFsm> FsmList;
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
    protected AutomationBase(IHaContext context, AutomationConfig config, ILogger logger)
    {
        Context = context;
        Logger = logger;
        Config = config;
        IsEnabledObserver = Observable.FromEventPattern<bool>(
            handler => IsEnabledChanged += handler,
            handler => IsEnabledChanged -= handler
        ).Select(pattern => pattern.EventArgs);
        FsmList = new List<TFsm>();
        EntitiesList = Config.Entities.OfType<TEntity>().ToArray() ?? [];
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
    
    protected void CreateFsm()
    {
        Logger.LogDebug("Configuring {FsmName} ", typeof(TFsm).Name);
        foreach (var blind in EntitiesList)
            FsmList.Add(ConfigureFsm(blind));
    }
    
    protected abstract TFsm ConfigureFsm(TEntity entity);
    
    private IObservable<StateChange> EntityEvent(string id) => Context.StateAllChanges().Where(e => e.New?.EntityId == id);
    
    protected IObservable<StateChange> UserEvent(string id) => EntityEvent(id)
        .Where(e => !e.IsAutomationInitiated(Config.ServiceAccountId));
    
    protected IObservable<StateChange> AutomationEvent(string id) => EntityEvent(id)
        .Where(e => e.IsAutomationInitiated(Config.ServiceAccountId));
    
    protected static void ChooseAction(bool condition, Action action, Action elseAction) => (condition ? action : elseAction)();
    
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