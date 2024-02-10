using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.FSM.LightFsm;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.Room.Core;

public enum LightState
{
    OnByMotion,
    OnBySwitch,
    Off,
    OffBySwitch
}

public enum LightTrigger
{
    MotionOnTrigger,
    MotionOffTrigger,
    SwitchOnTrigger,
    SwitchOffTrigger,
    TimerElapsed,
    AllOff
}

public static class LightFsmBaseExtensionMethods
{
    public static void FireAllOff(this IEnumerable<LightFsmBase> state)
    {
        foreach (var fsm in state)
        {
            fsm.FireAllOff();
        }
    }
}

public class LightFsmBase : IFsmBase
{
    private StateMachine<LightState, LightTrigger> _fsm;
    public ILightEntityCore Light { get; set; }
    public LightState State => _fsm.State;
    private AutomationConfig Config { get; set;}
    private ILogger Logger { get; set; }
    
    protected record JsonStorageSchema(LightState State);
    
    public LightFsmBase(ILightEntityCore light, Func<bool> sensorConditions, AutomationConfig config, ILogger logger)
    {
        Config = config;
        Light = light;
        Logger = logger;
        StoragePath = $"storage/v1/{light.EntityId}_fsm.json";
        Timer = new CustomTimer(logger);
        // _fsm = new StateMachine<LightState, LightTrigger>(LightState.Off);
        _fsm = new StateMachine<LightState, LightTrigger>(GetStateFromStorage, StoreState);
        
        _fsm.Configure(LightState.Off)
            .OnEntry(Timer.Dispose)
            .Ignore(LightTrigger.TimerElapsed)
            .PermitReentry(LightTrigger.MotionOffTrigger)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .PermitReentry(LightTrigger.AllOff)
            .PermitIf(LightTrigger.MotionOnTrigger, LightState.OnByMotion, sensorConditions)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch);
        
        _fsm.Configure(LightState.OnByMotion)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);
        
        _fsm.Configure(LightState.OnBySwitch)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);
        
        _fsm.Configure(LightState.OffBySwitch)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .Ignore(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Ignore(LightTrigger.TimerElapsed)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.AllOff, LightState.Off);
    }
    
    private void StoreState(LightState state)
    {
        Logger.LogDebug("Storing state in storage ({Path}) {State}", StoragePath, state);
        File.WriteAllText(StoragePath, "{\"State\": " + JsonConvert.SerializeObject(state) + "}");
    }
    
    private LightState GetStateFromStorage()
    {   
        Logger.LogInformation("Getting state from storage ({Path})", StoragePath);
        if (!File.Exists(StoragePath))
        {
            Logger.LogDebug("Storage file does not exist, creating new one");
            File.Create(StoragePath).Dispose();
            return LightState.Off;
        }
        var content = File.ReadAllText(StoragePath);
        var jsonContent = JsonConvert.DeserializeObject<JsonStorageSchema>(content);
        if (jsonContent != null)
        {
            Logger.LogDebug("Storage file content: {Content}", jsonContent);
            return jsonContent.State;
        }
        Logger.LogError("Could not deserialize storage file content");
        return LightState.Off;
    }
    
    protected override void ConfigureFsm()
    {
        throw new NotImplementedException();
    }

    public void FireMotionOff()
    {
        _fsm.Fire(LightTrigger.MotionOffTrigger);
    }

    public void FireMotionOn()
    {
        _fsm.Fire(LightTrigger.MotionOnTrigger);
    }

    public override void FireOn()
    {
        _fsm.Fire(LightTrigger.SwitchOnTrigger);
    }

    public override void FireOff()
    {
        _fsm.Fire(LightTrigger.SwitchOffTrigger);
    }

    public void FireAllOff()
    {
        _fsm.Fire(LightTrigger.AllOff);
    }
    
    public void FireTimerElapsed()
    {
        _fsm.Fire(LightTrigger.TimerElapsed);
    }
}