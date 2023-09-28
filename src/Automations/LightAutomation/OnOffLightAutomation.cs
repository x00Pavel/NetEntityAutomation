using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

/// <summary>
/// This automation expects that provided switch has 'on' and 'off' commands.
/// </summary>
public class OnOffLightAutomation: LightAutomation<OnOffFsmState>
{   
    private readonly MotionSwitchLightFsm _fsm;

    public OnOffLightAutomation(IHaContext ha, ILightAutomationConfig<OnOffFsmState> config, ILogger logger): base(logger, config, ha)
    {
        _fsm = new MotionSwitchLightFsm(logger, Config.FsmConfig)
        {
            TimerTrigger = OnOffFsmTrigger.TimeElapsed,
            StoragePath = $"storage/{config.Name}_fsm.json"
        };
        InitFsmTransitions();
        logger.LogInformation("{AutomationName} initialised", nameof(OnOffLightAutomation));
    }

    protected sealed override void InitFsmTransitions()
    {
        Logger.LogInformation("Initialising FSM for {RoomName}", Config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ => _fsm.MotionOn());
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ => _fsm.MotionOff());
        SwitchEvent.Where(e => e.Command == "on").Subscribe(_ => _fsm.SwitchOn());
        SwitchEvent.Where(e => e.Command == "off").Subscribe(_ => _fsm.SwitchOff());
    }
}