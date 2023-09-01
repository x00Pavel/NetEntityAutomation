using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public class BaseFsm<TState, TTRigger> where TState : Enum where TTRigger : Enum
{
    protected readonly ILogger Logger;
    protected readonly FsmConfig<TState> Config;
    protected IDisposable? Timer;
    private const string StoragePath = "storage/fsm.json";

    public TState State => StateMachine.State;
    protected StateMachine<TState, TTRigger> StateMachine;

    protected BaseFsm(ILogger logger, FsmConfig<TState> config)
    {
        Logger = logger;
        Config = config;
        logger.LogDebug("FSM configuration: {Config}", Config);
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
}