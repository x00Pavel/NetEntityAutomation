using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class BaseFsm<TState, TTRigger>
    where TState : Enum where TTRigger : Enum
{
    protected readonly ILogger Logger;
    protected readonly FsmConfig<TState> Config;
    protected IDisposable? Timer;
    public string? StoragePath { get; init; }
    public TState State => StateMachine.State;
    protected StateMachine<TState, TTRigger> StateMachine;
    public required TTRigger TimerTrigger { get; init; }

    protected BaseFsm(ILogger logger, FsmConfig<TState> config)
    {
        Logger = logger;
        Config = config;
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
        Logger.LogInformation("Updating state in storage {State}", State);
        File.WriteAllText(StoragePath, ToJson());
    }

    private string ToJson()
    {
        Logger.LogInformation("Serializing to JSON");
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFsm? FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFsm>(jsonString);
    }

    protected void StartTimer(TimeSpan waitTime)
    {
        Logger.LogInformation("Starting timer for {WaitTime} seconds", waitTime.Seconds);
        Timer?.Dispose();
        Timer = Observable.Timer(waitTime)
            .Subscribe(_ => TimeElapsed());
    }

    private void TimeElapsed()
    {
        try
        {
            Logger.LogInformation("Time elapsed");
            StateMachine.Fire(TimerTrigger);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }

    protected abstract void InitFsm();
}