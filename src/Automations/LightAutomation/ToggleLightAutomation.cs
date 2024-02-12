using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.ExtensionMethods;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public class ToggleLightAutomation: LightAutomation<ToggleFsmState>
{   
    private readonly ToggleLightFsm _lightFsm;
    
    public ToggleLightAutomation(IHaContext ha, ILightAutomationConfiguration<ToggleFsmState> config, ILogger logger): base(logger, config, ha)
    {   
        _lightFsm = new ToggleLightFsm(config: config.FsmConfig, logger: logger, storageFileName: config.ProgramName)
        {
            TimerTrigger = ToggleFsmTrigger.TimeElapsed,
        };
        InitFsmTransitions();
        logger.LogInformation("{AutomationName} initialised", nameof(ToggleLightAutomation));
    }
    
    private void TurnOnLights(TimeSpan waitTime)
    {   
        Logger.LogInformation("Turning on lights");
        if (Config.NightMode is { IsEnabled: true, IsWorkingHours: true })
        {   
            Config.NightMode.Devices?.TurnOn(Config.NightMode.LightParameters);
        }
        else
        {
            Config.Lights.TurnOn(transition: Config.Transition);    
        }
        // StartTimer(waitTime);
    }
    
    private void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        Config.Lights.TurnOff();
        _lightFsm.Timer?.Dispose();
    }
    
    protected sealed override void InitFsmTransitions()
    {
        Logger.LogInformation("Initialising FSM for {RoomName}", Config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ =>
        {
            TurnOnLights(_lightFsm.Config.HoldOnTime);
            // _lightFsm.MotionOn(ToggleFsmTrigger.MotionOn);
        });
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ =>
        {
            TurnOffLights();
            // _lightFsm.MotionOff(ToggleFsmTrigger.MotionOff);
        });
        ZhaSwitchEvent.Where(e => e.Command == "toggle").Subscribe(_ =>
        {
            if(_lightFsm.State == ToggleFsmState.Off)
                TurnOnLights(_lightFsm.Config.HoldOnTime);
            else
                TurnOffLights();
            // _lightFsm.Toggle();
        });
        IsEnabledObserver.Subscribe(value => _lightFsm.IsEnabled = value);
    }

    // private void StartTimer(TimeSpan waitTime)
    // {
    //     Logger.LogDebug("Starting timer for {WaitHours}:{WaitMinutes}:{WaitTime}", waitTime.Hours, waitTime.Minutes, waitTime.Seconds);
    //     _lightFsm.Timer?.Dispose();
    //     _lightFsm.Timer = Observable.Timer(waitTime)
    //         .Subscribe(_ => _lightFsm.TimeElapsed());
    // }
    
}