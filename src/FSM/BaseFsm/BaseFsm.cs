using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class BaseFsm<TState, TTRigger>
    where TState : Enum where TTRigger : Enum
{
    protected readonly ILogger Logger;
    protected readonly IFsmConfig<TState> Config;
    protected IDisposable? Timer;
    public required string StoragePath { get; init; }
    
    public TState State => StateMachine.State;
    protected readonly StateMachine<TState, TTRigger> StateMachine;
    public required TTRigger TimerTrigger { get; init; }

    protected BaseFsm(ILogger logger, IFsmConfig<TState> config)
    {
        Logger = logger;
        Config = config;
        logger.LogDebug("Night mode enabled: {Enabled}", config.NightMode);
        logger.LogDebug("FSM configuration: {Config}", Config);
        StateMachine = new StateMachine<TState, TTRigger>(Config.InitialState);
        InitFsm();
    }

    protected bool WorkingHours()
    {
        var now = DateTime.Now.TimeOfDay;
        Logger.LogDebug("Working hors: {Start} - {Stop}", Config.StartAtTimeFunc(), Config.StopAtTimeFunc());
        Logger.LogDebug("Is working {Now} hours: {IsWorkingHours}", now, Config.IsWorkingHours);
        return Config.IsWorkingHours;
    }

    protected void UpdateState()
    {
        Logger.LogDebug("Updating state in storage {State}", State);
        File.WriteAllText(StoragePath, ToJson());
    }

    private string ToJson()
    {
        Logger.LogDebug("Serializing to JSON");
        return JsonConvert.SerializeObject(this);
    }
    
    // FIXME: This is not working because of circular dependency
    // public static BaseFsm? FromJson(string jsonString)
    // {
    //     return JsonConvert.DeserializeObject<MotionSwitchLightFsm>(jsonString);
    // }

    protected void StartTimer(TimeSpan waitTime)
    {
        Logger.LogDebug("Starting timer for {WaitTime} seconds", waitTime.TotalSeconds);
        Timer?.Dispose();
        Timer = Observable.Timer(waitTime)
            .Subscribe(_ => TimeElapsed());
    }

    private void TimeElapsed()
    {
        try
        {
            Logger.LogDebug("Time elapsed");
            StateMachine.Fire(TimerTrigger);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogError(e.Message);
        }
    }
    
    protected bool UserDefinedGuard()
    {
        return Config.AdditionalConditions
            .Select(func => func())
            .ToList()
            .All(e => e);
    }

    protected abstract void InitFsm();
}