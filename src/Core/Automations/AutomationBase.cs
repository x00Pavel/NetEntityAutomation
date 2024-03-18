using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Conditions;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Core.Triggers;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Core.Automations;


public abstract class AutomationBase: IAutomationConfig
{
    private bool _enabled = true;
    public readonly IObservable<bool> IsEnabledObserver;
    public event EventHandler<bool>? IsEnabledChanged;
    public IEnumerable<IStateChangeTrigger> Triggers { get; set; }
    public IEnumerable<ICondition> Conditions { get; set; } = new []{new DefaultCondition()};
    public string ServiceAccountId { get; set; }
    public TimeSpan WaitTime { get; set; }
    public TimeSpan SwitchTimer { get; set; }
    public Func<TimeSpan>? StopAtTimeFunc { get; set; }
    public Func<TimeSpan>? StartAtTimeFunc { get; set; }
    public NightModeConfig NightMode { get; set; }
    public IHaContext Context { get; set; }
    public ILogger Logger { get; set; }
    public abstract void ConfigureAutomation();
    
    public bool IsEnabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            IsEnabledChanged?.Invoke(this, value);
        }
    }
    protected AutomationBase()
    {
        IsEnabledObserver = Observable.FromEventPattern<bool>(
            handler => IsEnabledChanged += handler,
            handler => IsEnabledChanged -= handler
        ).Select(pattern => pattern.EventArgs);
    }
}

/// <summary>
 /// This class represents a base for all automations.
 /// The automation works with certain type of entities and uses Finite State Machine to store and represent the state of the entities.
 /// </summary>
 /// <typeparam name="TEntity">Type of entities an automation will work with</typeparam>
 /// <typeparam name="TFsm">Type of Finite State Machine that will be used for storing and representing the state of TEntity</typeparam>
public abstract class AutomationBase<TEntity, TFsm>: AutomationBase
{
    public List<TEntity> EntitiesList { get; set; }
    protected List<TFsm> FsmList { get; } = [];
    
    /// <summary>
    /// Helper method to trigger an event at a specific time of the day.
    /// It uses Observable.Timer to trigger the event.
    /// This means that the relative time is calculated from now to the specified time.
    /// </summary>
    /// <param name="timeSpan">When the event should be triggered</param>
    /// <param name="action">Callable with no arguments to be called when an event is triggered</param>
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
    
    /// <summary>
    /// Creates a Finite State Machine for each entity in the room.
    /// </summary>
    protected void CreateFsm()
    {
        Logger.LogDebug("Configuring {FsmName} ", typeof(TFsm).Name);
        foreach (var entity in EntitiesList)
            FsmList.Add(ConfigureFsm(entity));
    }
    
    /// <summary>
    /// Abstract method to configure a Finite State Machine for a specific entity.
    /// Each automation have to implement this method to configure the Finite State Machine for the specific entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected abstract TFsm ConfigureFsm(TEntity entity);
    
    /// <summary>
    /// Observable for all state changes of a specific entity.
    /// </summary>
    /// <param name="id">HomeAssistant ID of the entity</param>
    /// <returns>Observable of StateChange</returns>
    protected IObservable<StateChange> EntityEvent(string id) => Context.StateAllChanges().Where(e => e.New?.EntityId == id);
    
    /// <summary>
    /// Observable for all state changes of a specific entity initiated by a user.
    /// 
    /// </summary>
    /// <remarks>
    /// To determine if the state change was initiated by a user, the method checks if the state change was initiated by the service account.
    /// The service account is specified in configuration of the automation or, if not specified, it is the default service account (called NetDaemon) is used.
    /// </remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    protected IObservable<StateChange> UserEvent(string id) => EntityEvent(id)
        .Where(e => !e.IsAutomationInitiated(ServiceAccountId));

    /// <summary>
    /// Observable for all state changes of a specific entity initiated by the automation.
    /// This method uses ServiceAccountId to determine if the state change was initiated by the automation.
    /// </summary>
    /// <param name="id">Account ID which owns the token for NetDaemon</param>
    /// <returns></returns>
    protected IObservable<StateChange> AutomationEvent(string id) => EntityEvent(id)
        .Where(e => e.IsAutomationInitiated(ServiceAccountId));
    
    /// <summary>
    /// Helper function to choose between two actions based on a condition.
    /// Practically, it is a shorthand for if-else statement or a ternary operator.
    /// </summary>
    /// <param name="condition">Boolean value. It is up to caller execute the logic.</param>
    /// <param name="action">Action if condition == true</param>
    /// <param name="elseAction">Action on else branch</param>
    protected static void ChooseAction(bool condition, Action action, Action elseAction) => (condition ? action : elseAction)();
    
    /// <summary>
    /// Helper observable for making actions when automation is disabled.
    /// </summary>
    public IObservable<bool> AutomationDisabled => IsEnabledObserver.Where(enabled => !enabled);
    
    /// <summary>
    /// Helper observable for making actions when automation is enabled.
    /// </summary>
    public IObservable<bool> AutomationEnabled => IsEnabledObserver.Where(enabled => enabled);
    
    protected bool IsWorkingHours()
    {
        if (NightMode is { StartAtTimeFunc: not null, StopAtTimeFunc: not null })
            return UtilsMethods.NowInTimeRange(StartAtTimeFunc(), StopAtTimeFunc());
        Logger.LogWarning("Can't determine working hours. StartAtTimeFunc or StopAtTimeFunc is not set");
        return true;
    }

    protected CustomTimer? ResetTimerOrAction(CustomTimer timer, TimeSpan time, Action action, Func<bool> resetCondition)
    {
        if (resetCondition())
        {
            Logger.LogDebug("Resetting timer with time {Time}", time);
            timer.StartTimer(time, () => ResetTimerOrAction(timer, time, action, resetCondition));
            return timer;
        }
        Logger.LogDebug("Doing action {Action}", action.Method.Name);
        action();
        return null;
    }
}
