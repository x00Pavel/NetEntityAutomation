using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class BaseLightFsm<TState, TTrigger>
    where TState : Enum where TTrigger : Enum
{
    protected record JsonStorageSchema(TState State);

    protected readonly ILogger Logger;
    protected readonly IFsmConfig<TState> Config;
    protected IDisposable? Timer;
    private string StoragePath { get; init; }
    
    protected TState State => StateMachine.State;
    protected readonly StateMachine<TState, TTrigger> StateMachine;
    public required TTrigger TimerTrigger { get; init; }
    public bool IsEnabled { get; set; } = true;
    
    protected BaseLightFsm(ILogger logger, IFsmConfig<TState> config, string storageFileName)
    {
        Logger = logger;
        Config = config;
        StoragePath = $"storage/{storageFileName}_fsm.json";
        logger.LogDebug("Night mode enabled: {Enabled}", config.NightMode);
        logger.LogDebug("FSM configuration: {Config}", Config);
        StateMachine = new StateMachine<TState, TTrigger>(GetStateFromStorage, StoreState);
        InitFsm();
        Logger.LogDebug("Current state of the FSM: {State}", StateMachine.State);
        StateMachine.Activate();
    }

    private void StoreState(TState state)
    {
        Logger.LogDebug("Storing state in storage ({Path}) {State}", StoragePath, state);
        File.WriteAllText(StoragePath, "{\"State\": " + JsonConvert.SerializeObject(state) + "}");
    }
    
    private TState GetStateFromStorage()
    {   
        Logger.LogInformation("Getting state from storage ({Path})", StoragePath);
        if (!File.Exists(StoragePath))
        {
            Logger.LogDebug("Storage file does not exist, creating new one");
            File.Create(StoragePath).Dispose();
            return Config.InitialState;
        }
        var content = File.ReadAllText(StoragePath);
        var jsonContent = JsonConvert.DeserializeObject<JsonStorageSchema>(content);
        if (jsonContent != null)
        {
            Logger.LogDebug("Storage file content: {Content}", jsonContent);
            return jsonContent.State;
        }
        Logger.LogError("Could not deserialize storage file content");
        return Config.InitialState;
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