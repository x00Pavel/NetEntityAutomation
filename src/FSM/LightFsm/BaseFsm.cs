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
    public bool IsEnabled { get; set; } = true;
    
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
        Logger.LogDebug("Working hours: {Start} - {Stop}", Config.StartAtTimeFunc(), Config.StopAtTimeFunc());
        Logger.LogDebug("Is working {Now} hours: {IsWorkingHours}", now, Config.IsWorkingHours);
        return Config.IsWorkingHours;
    }

    protected bool SensorConditions()
    {   
        var result = WorkingHours() && Config.SensorConditionMet && IsEnabled; 
        Logger.LogDebug("Sensor conditions met: {Conditions}", result);
        return result;
    }
    
    protected void UpdateState()
    {
        Logger.LogDebug("Updating state in storage {State}", State);
        Logger.LogDebug("State of FSM is {Enabled}", IsEnabled);
        File.WriteAllText(StoragePath, ToJson());
    }

    private string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFsm? FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFsm>(jsonString);
    }

    protected void StartTimer(TimeSpan waitTime)
    {
        Logger.LogDebug("Starting timer for {WaitHours}:{WaitMinutes}:{WaitTime}", waitTime.Hours, waitTime.Minutes, waitTime.Seconds);
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

    protected abstract void InitFsm();
}