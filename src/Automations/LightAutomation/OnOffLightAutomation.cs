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
    private readonly OnOffLightFsm _lightFsm;

    public OnOffLightAutomation(IHaContext ha, IAutomationConfig<OnOffFsmState> config, ILogger logger): base(logger, config, ha)
    {
        _lightFsm = new OnOffLightFsm(logger, Config.FsmConfig, Config.ProgramName)
        {
            TimerTrigger = OnOffFsmTrigger.TimeElapsed,
        };
        InitFsmTransitions();
        logger.LogInformation("LightAutomation initialised");
    }

    protected sealed override void InitFsmTransitions()
    {
        Logger.LogInformation("Initialising FSM for {RoomName}", Config.Name);
        MotionSensorEvent.Where(e => e.New?.State == "on").Subscribe(_ => _lightFsm.MotionOn(OnOffFsmTrigger.MotionOn));
        MotionSensorEvent.Where(e => e.New?.State == "off").Subscribe(_ => _lightFsm.MotionOff(OnOffFsmTrigger.MotionOff));
        SwitchEvent.Where(e => e.Command == "on").Subscribe(_ => _lightFsm.SwitchOn());
        SwitchEvent.Where(e => e.Command == "off").Subscribe(_ => _lightFsm.SwitchOff());
        BaseLightFsm
    }
}