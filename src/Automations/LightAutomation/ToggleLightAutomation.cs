using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public class ToggleLightAutomation: LightAutomation<ToggleFsmState>
{   
    private readonly ToggleFsm _fsm;
    
    public ToggleLightAutomation(IHaContext ha, ILightAutomationConfig<ToggleFsmState> config, ILogger logger): base(logger, config, ha)
    {   
        _fsm = new ToggleFsm(config: config.FsmConfig, logger: logger)
        {
            TimerTrigger = ToggleFsmTrigger.TimeElapsed,
            StoragePath = $"storage/{config.Name}_fsm.json"
        };
        InitFsmTransitions();
        logger.LogInformation("{AutomationName} initialised", nameof(ToggleLightAutomation));
    }

    protected sealed override void InitFsmTransitions()
    {
        Logger.LogInformation("Initialising FSM for {RoomName}", Config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ => _fsm.MotionOn());
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ => _fsm.MotionOff());
        SwitchEvent.Where(e => e.Command == "toggle").Subscribe(_ => _fsm.Toggle());
    }
}