using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Fsm;
using NetEntityAutomation.Core.Triggers;
using NetEntityAutomation.Extensions.ExtensionMethods;
using Newtonsoft.Json;

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
    private static LightParameters DefaultLightParameters => new() {Brightness = 255};

    public override void ConfigureAutomation()
    {
        CreateFsm();
        Logger.LogDebug("Subscribing to motion sensor events");

        foreach (var sensor in Triggers)
        {
            sensor.On.Subscribe(e => { if (IsEnabled) TurnOnByAutomation(e.Entity); });
        }
        
        AutomationDisabled.Subscribe(e =>
        {
            Logger.LogDebug("Disabling automation {Automation}", nameof(LightAutomationBase));
            foreach (var fsm in LightOnByAutomation)
            {
                fsm.Timer.Dispose();
            }
        });
        
        AutomationEnabled.Subscribe(e =>
        {
            Logger.LogDebug("Enabling automation {Automation}", nameof(LightAutomationBase));
            if (Triggers.IsAllOn())
                EntitiesList.ForEach(TurnOnByAutomation);
        });
    }
    
    private void TurnOnByAutomation(IEntityCore? entityCore)
    {
        if (entityCore == null)
        {
            throw new NullReferenceException("Light entity is null. Cannot turn on lights by motion sensor.");
        }
        if (!IsWorkingHours())
        {
            Logger.LogDebug("Turning on lights by motion sensor {Sensor} is not allowed because it's not working hours",
                entityCore.EntityId);
            return;
        }

        if (!Conditions.All(c => c.IsTrue()))
        {
            Logger.LogDebug("Not all conditions are met to turn on lights by motion sensor {Sensor}",
                entityCore.EntityId);
            return; 
        }

        Logger.LogDebug("Turning on lights by motion sensor {Sensor}", entityCore.EntityId);
        switch (NightMode)
        {
            case { IsEnabled: true, IsWorkingHours: true }:
                Logger.LogDebug("Time of Night Mode {Time}", DateTime.Now.TimeOfDay);
                foreach (var light in LightsOffByAutomation.Select(fsm => fsm.Entity))
                {
                    if (NightMode.Devices?.Contains(light) ?? false)
                        light.TurnOn(NightMode.LightParameters);
                }
                break;
            case { IsEnabled: true, IsWorkingHours: false }:
                Logger.LogDebug("Normal working hours {Time}", DateTime.Now.TimeOfDay);
                foreach (var fsm in LightsOffByAutomation)
                {
                    var light = fsm.Entity;
                    var par = fsm.LastParams ?? DefaultLightParameters;
                    Logger.LogDebug("Restoring light parameters for light {Light} : {LightParams}", light.EntityId, par);
                    light.TurnOn(par);
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

    private bool OnConditionsMet() => IsWorkingHours() && Triggers.Any(s => s.IsOn()) && Conditions.All(c => c.IsTrue());

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
                    ResetTimerOrAction(lightFsm.Timer, WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
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
                    ResetTimerOrAction(lightFsm.Timer, WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
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
        var lightFsm = new LightFsmBase(l, Logger).Configure(ActionForLight(l));
        
        AutomationOn(l.EntityId)
            .Subscribe(e =>
            {
                Logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                lightFsm.FireMotionOn();
                // This is needed to reset the timer if timeout is expired, but there is still a motion
                ResetTimerOrAction(lightFsm.Timer, WaitTime, lightFsm.Entity.TurnOff, OnConditionsMet);
            });
        AutomationOff(l.EntityId)
            .Subscribe(e =>
            {
                Logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                if (!NightMode.IsWorkingHours)
                {
                    Logger.LogDebug("Storing light parameters for light {Light} : {LightParams}", l.EntityId, e.Old?.AttributesJson);
                    lightFsm.LastParams =
                        JsonConvert.DeserializeObject<LightParameters>(e.Old?.AttributesJson.ToString() ?? "{}");
                }
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
                lightFsm.Timer.StartTimer(SwitchTimer, lightFsm.Entity.TurnOff);
            });
        UserOff(l.EntityId)
            .Subscribe(e =>
                {
                    Logger.LogDebug("Light Event: lights {Light} is in {State} by user {User}",
                        e.New?.EntityId,
                        e.New?.State,
                        e.New?.Context?.UserId);
                    if (!NightMode.IsWorkingHours)
                    {
                        Logger.LogDebug("Storing light parameters for light {Light} : {LightParams}", l.EntityId, e.Old?.AttributesJson);
                        lightFsm.LastParams =
                            JsonConvert.DeserializeObject<LightParameters>(e.Old?.AttributesJson.ToString() ?? "{}");
                    }
                    
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