using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Core.Fsm;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Core.Automations;

/// <summary>
/// Abstract class for managing light automations.
/// Scenario and triggers in this class created with secondary light use-case in mind.
/// <br/>
/// Scenario:
/// <list type="bullet">
///     <item>
///         <description>Specific lights triggered by the motion sensor in certain time frame specified by <c>StopAtTimeFunc</c> and <c>StartAtTimeFunc</c> callable parameters in the config.</description>
///     </item>
/// </list>
/// </summary>
public class LightAutomationBase : AutomationBase<ILightEntityCore, LightFsmBase>
{
    private IDictionary<string, LightParameters> _lightParameters = new Dictionary<string, LightParameters>();

    public LightAutomationBase(IHaContext context, AutomationConfig config, ILogger logger): base(context, config, logger)
    {
        CreateFsm();
        ConfigureAutomation();
    }

    private void ConfigureAutomation()
    {
        Logger.LogDebug("Subscribing to motion sensor events");

        foreach (var sensor in Config.Triggers)
        {
            sensor.On.Subscribe(TurnOnByAutomation);
        }
    }
    
    private void TurnOnByAutomation(StateChange e)
    {
        if (!IsWorkingHours())
        {
            Logger.LogDebug("Turning on lights by motion sensor {Sensor} is not allowed because it's not working hours",
                e.Entity.EntityId);
            return;
        }

        if (!Config.Conditions.All(c => c.IsTrue()))
        {
            Logger.LogDebug("Not all conditions are met to turn on lights by motion sensor {Sensor}",
                e.Entity.EntityId);
            return; 
        }

        Logger.LogDebug("Turning on lights by motion sensor {Sensor}", e.Entity.EntityId);
        switch (Config.NightMode)
        {
            case { IsEnabled: true, IsWorkingHours: true }:
                Logger.LogDebug("Time of Night Mode {Time}", DateTime.Now.TimeOfDay);
                foreach (var fsm in LightsOffByAutomation)
                {
                    if (Config.NightMode.Devices?.Contains(fsm.Entity) ?? false)
                    {
                        _lightParameters[fsm.Entity.EntityId] = fsm.Entity.GetLightParameters() ?? new LightParameters
                        {
                            Brightness = 255
                        };
                    
                        fsm.Entity.TurnOn(Config.NightMode.LightParameters);    
                    }
                }
                Logger.LogDebug("Stored values for light {Light}", _lightParameters);
                break;
            case { IsEnabled: true, IsWorkingHours: false }:
                Logger.LogDebug("Normal working hours {Time}", DateTime.Now.TimeOfDay);
                if (_lightParameters.Count > 0)
                {
                    // Restore light parameters after night mode
                    foreach (var fsm in LightsOffByAutomation)
                    {
                        var lightParams = _lightParameters.TryGetValue(fsm.Entity.EntityId, out var parameters)
                            ? parameters
                            : new LightParameters
                            {
                                Brightness = 255
                            };
                        fsm.Entity.TurnOn(lightParams);
                        _lightParameters.Remove(fsm.Entity.EntityId);
                    }
                    Logger.LogDebug("Idle values {Light}", _lightParameters);
                }
                else
                {
                    LightsOffByAutomation.Select(fsm => fsm.Entity).TurnOn();
                }
                break;
            case { IsEnabled: false }:
                LightsOffByAutomation.Select(fsm => fsm.Entity).TurnOn();
                break;
            default:
                Logger.LogDebug("Not working hours {Time}", DateTime.Now.TimeOfDay);
                break;
        }
    }

    private bool OnConditionsMet() => IsWorkingHours() && Config.Triggers.Any(s => s.IsOn()) && Config.Conditions.All(c => c.IsTrue());

    private LightStateActivateAction ActionForLight(ILightEntityCore l)
    {
        return new LightStateActivateAction
        { 
            OffAction = _ =>
            {
                Logger.LogDebug("Activating FSM in state Off ");
                l.TurnOff();
            },
            OnByMotionAction = lightFsm =>
            {
                Logger.LogDebug("Activating FSM in state OnByMotion");
                if (OnConditionsMet())
                {
                    l.TurnOn();
                    ResetTimerOrAction(lightFsm.Timer, Config.WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
                }
                else
                {
                    l.TurnOff();
                }
            },
            OnBySwitchAction = lightFsm => 
            {
                Logger.LogDebug("Activating FSM in state OnBySwitch");  
                if (OnConditionsMet())
                {
                    l.TurnOn();
                    ResetTimerOrAction(lightFsm.Timer, Config.WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
                }
                else
                {
                    l.TurnOff();
                }
                
            },
            OffBySwitchAction = _ =>
            {
                Logger.LogDebug("Activating FSM in state OffBySwitch");
                l.TurnOff();
            }
        };
    }
    
    protected override LightFsmBase ConfigureFsm(ILightEntityCore l)
    {
        var lightFsm = new LightFsmBase(l, Config, Logger).Configure(ActionForLight(l));
        
        AutomationOn(l.EntityId)
            .Subscribe(e =>
            {
                Logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                lightFsm.FireMotionOn();
                // This is needed to reset the timer if timeout is expired, but there is still a motion
                ResetTimerOrAction(lightFsm.Timer, Config.WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
            });
        AutomationOff(l.EntityId)
            .Subscribe(e =>
            {
                Logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                
                ChooseAction(OnLights > 0, lightFsm.FireMotionOff, FsmList.FireAllOff);
            });

        UserOn(l.EntityId)
            .Subscribe(e =>
            {
                Logger.LogDebug("Light Event: lights {Light} is in {State} by user {User}",
                    e.New?.EntityId,
                    e.New?.State,
                    e.New?.Context?.UserId);
                lightFsm.FireOn();
                lightFsm.Timer.StartTimer(Config.SwitchTimer, lightFsm.Entity.TurnOff);
            });
        UserOff(l.EntityId)
            .Subscribe(e =>
                {
                    Logger.LogDebug("Light Event: lights {Light} is in {State} by user {User}",
                        e.New?.EntityId,
                        e.New?.State,
                        e.New?.Context?.UserId);
                    ChooseAction(OnLights > 0, lightFsm.FireOff, FsmList.FireAllOff);
                }
            );
        return lightFsm;
    }

    private int OnLights => EntitiesList.Count(l => Context.GetState(l.EntityId)?.State == "on");

    private IEnumerable<LightFsmBase> LightsOffByAutomation =>
        FsmList.Where(fsm => fsm.State != LightState.OffBySwitch);

    private IEnumerable<LightFsmBase> LightOnByAutomation => FsmList.Where(fsm => fsm.State != LightState.OnBySwitch);
    
    private IObservable<StateChange> UserOn(string id) => UserEvent(id)
        .Where(e => e.New?.State == "on");

    private IObservable<StateChange> UserOff(string id) => UserEvent(id)
        .Where(e => e.New?.State == "off");
    
    private IObservable<StateChange> AutomationOn(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "on");

    private IObservable<StateChange> AutomationOff(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "off");
}