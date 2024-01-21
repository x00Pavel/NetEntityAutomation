using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public class ToggleLightAutomation: LightAutomation<ToggleFsmState>
{   
    private readonly ToggleLightFsm _lightFsm;
    
    public ToggleLightAutomation(IHaContext ha, IAutomationConfig<ToggleFsmState> config, ILogger logger): base(logger, config, ha)
    {   
        _lightFsm = new ToggleLightFsm(config: config.FsmConfig, logger: logger, storageFileName: config.ProgramName)
        {
            TimerTrigger = ToggleFsmTrigger.TimeElapsed,
        };
        InitFsmTransitions();
        logger.LogInformation("{AutomationName} initialised", nameof(ToggleLightAutomation));
    }

    protected sealed override void InitFsmTransitions()
    {
        Logger.LogInformation("Initialising FSM for {RoomName}", Config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ => _lightFsm.MotionOn(ToggleFsmTrigger.MotionOn));
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ => _lightFsm.MotionOff(ToggleFsmTrigger.MotionOff));
        SwitchEvent.Where(e => e.Command == "toggle").Subscribe(_ => _lightFsm.Toggle());
        IsEnabledObserver.Subscribe(value => _fsm.IsEnabled = value);
    }
}