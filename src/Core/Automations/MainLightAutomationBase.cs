using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Core.Fsm;
using NetEntityAutomation.Core.Triggers;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Core.Automations;

public class MainLightAutomationBase: AutomationBase<ILightEntityCore, MainLightFsmBase>
{
    private readonly IEnumerable<ILightEntityCore> _lights;
    private readonly IEnumerable<MotionSensor> _motionSensors;
    private readonly CustomTimer _timer;
    
    public MainLightAutomationBase(IHaContext context, AutomationConfig config, ILogger logger) : base(context, config, logger)
    {
        _lights = config.Entities.OfType<ILightEntityCore>();
        _motionSensors = config.Triggers.OfType<MotionSensor>();
        _timer = new CustomTimer(Logger);
        CreateFsm();
        ConfigureAutomation();
    }

    /// <summary>
    /// The automation scenario is to turn off the main light in specific time frame specified by
    /// <c>Config.WaitTime</c> parameter.
    /// On the timeout, if there are no triggers in on state (usually, this is a motion sensor), the light will be turned off.
    /// </summary>
    private void ConfigureAutomation()
    {
        foreach (var light in _lights)
        {
            light.OnEvent().Subscribe(e =>
            {
                Observable
                    .Timer(Config.WaitTime)
                    .Subscribe(_ => ResetTimerOrAction(_timer, Config.WaitTime, light.TurnOff, _motionSensors.IsAnyOn));
            });
            light.OffEvent()
                .Subscribe(e => _timer.Dispose());
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// <para>
    /// For main light there is only two states: On and Off. The state of the light is fully controlled by the user.
    /// </para>
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    protected override MainLightFsmBase ConfigureFsm(ILightEntityCore entity)
    {   
        var lightFsm = new MainLightFsmBase(entity, Config, Logger).Configure(ActionForLight());
        entity.OnEvent().Subscribe(_ => lightFsm.FireOn());
        entity.OffEvent().Subscribe(_ => lightFsm.FireOff());
        return lightFsm;
    }
    
    /// <summary>
    /// For main automation there is no sense to check any conditions to turn off/on the light as the control over main
    /// light is fully on the user.
    /// </summary>
    /// <returns></returns>
    private static MainLightActivateAction ActionForLight()
    {
        return new MainLightActivateAction
        {
            OffAction = l => l.Entity.TurnOff(),
            OnAction = l => l.Entity.TurnOn()
        };
    }
}