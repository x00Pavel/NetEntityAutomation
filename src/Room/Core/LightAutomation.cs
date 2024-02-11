using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.Extensions.LightExtensionMethods;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Room.Core;

public interface IAutomation;

public class LightAutomation: IAutomation
{
    private readonly IEnumerable<ILightEntityCore> _lights;
    private readonly IHaContext _context;
    private readonly ILogger _logger;
    private readonly AutomationConfig _config;
    private readonly List<LightFsmBase> _fsmList = [];
    private int OnLights => _lights.Count(l => _context.GetState(l.EntityId)?.State == "on");
    private IEnumerable<LightFsmBase> LightsOffByAutomation => _fsmList.Where(fsm => fsm.State != LightState.OffBySwitch);
    private IEnumerable<LightFsmBase> LightOnByAutomation => _fsmList.Where(fsm => fsm.State != LightState.OnBySwitch);
    private IDictionary<string, LightParameters> _lightParameters = new Dictionary<string, LightParameters>();
    public LightAutomation(IHaContext context, AutomationConfig config, ILogger logger)
    {
        _config = config;
        _context = context;
        _logger = logger;
        _lights = config.Entities.OfType<ILightEntityCore>();
        
        CreateFsm();
        ConfigureTriggers();
    }

    private void ConfigureTriggers()
    {
        _logger.LogDebug("Subscribing to motion sensor events");

        foreach (var sensor in _config.Triggers)
        {   
            sensor.On.Subscribe(TurnOnByAutomation);
            // sensor.Off.Subscribe(TurnOffByAutomation);
        }
    }

    private void TurnOffByAutomation(StateChange e)
    {
        if (!IsWorkingHours())
        {
            _logger.LogDebug("Turning off lights by motion sensor {Sensor} is not allowed because it's not working hours", e.Entity.EntityId);
            return;
        }

        if (!_config.Conditions.All(c => c.IsTrue()))
        {
            _logger.LogDebug("Not all conditions are met to turn off lights by motion sensor {Sensor}", e.Entity.EntityId);
            return;
        }   
            
        _logger.LogDebug("Turning off lights by motion sensor {Sensor}", e.Entity.EntityId);
        LightOnByAutomation.Select(fsm => fsm.Light).TurnOff();
    }
    
    private void TurnOnByAutomation(StateChange e)
    {
        if (!IsWorkingHours())
        {
            _logger.LogDebug("Turning on lights by motion sensor {Sensor} is not allowed because it's not working hours", e.Entity.EntityId);
            return;
        }
        if (!_config.Conditions.All(c => c.IsTrue()))
        {
            _logger.LogDebug("Not all conditions are met to turn on lights by motion sensor {Sensor}", e.Entity.EntityId);
            return;
        }   
        
        _logger.LogDebug("Turning on lights by motion sensor {Sensor}", e.Entity.EntityId);
        switch (_config.NightMode)
        {
            case { IsEnabled: true, IsWorkingHours: true }:
                foreach (var fsm in LightsOffByAutomation)
                {
                    _lightParameters[fsm.Light.EntityId] = fsm.Light.GetLightParameters() ?? new LightParameters
                    {
                        BrightnessPct = 100
                    };
                    fsm.Light.TurnOn(_config.NightMode.LightParameters);
                }
                break;
            case { IsEnabled: true, IsWorkingHours: false }:
                if (_lightParameters.Count > 0)
                {
                    // Restore light parameters after night mode
                    foreach (var fsm in LightsOffByAutomation)
                    {
                        fsm.Light.TurnOn(_lightParameters[fsm.Light.EntityId]);
                        _lightParameters.Remove(fsm.Light.EntityId);
                    }
                }
                else
                {
                    LightsOffByAutomation.Select(fsm => fsm.Light).TurnOn();
                }
                break;
            case { IsEnabled: false}:
                LightsOffByAutomation.Select(fsm => fsm.Light).TurnOn();
                break;
            default:
                break;
        }
    }

    private bool IsWorkingHours()
    {
        var now = DateTime.Now.TimeOfDay;
        return now >= _config.StartAtTimeFunc() || now <= _config.StopAtTimeFunc();
    }
    private void CreateFsm()
    {
        foreach (var l in _lights)
            _fsmList.Add(ConfigureFsm(l));
    }
    
    private void ResetTimerOrDoAction(IFsmBase fsm, TimeSpan time, Action action, Func<bool> resetCondition)
    {   
        _logger.LogDebug("Resetting timer or doing action {Action} with time {Time}", action.Method.Name, time);
        if (resetCondition())
        {
            fsm.Timer.StartTimer(time, () => ResetTimerOrDoAction(fsm, time, action, resetCondition));
            return;
        }
        action();
    }
    
    private LightFsmBase ConfigureFsm(ILightEntityCore l)
    {
        var lightFsm = new LightFsmBase(l, _config, _logger);
        
        AutomationOn(l.EntityId)
            .Subscribe(e =>
            {
                _logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                lightFsm.FireMotionOn();
                // This is needed to reset the timer if timeout is expired, but there is still a motion
                ResetTimerOrDoAction(lightFsm, _config.WaitTime, lightFsm.Light.TurnOff, () => _config.Triggers.Any(s => s.IsOn()));
            });
        AutomationOff(l.EntityId)
            .Subscribe(e =>
            {
                _logger.LogDebug("Light Event: lights {Light} is in {State} without user by automation {User}",
                    e.New?.EntityId, e.New?.State, e.New?.Context?.UserId);
                if (OnLights == 0) _fsmList.FireAllOff();
                else lightFsm.FireMotionOff();
            });
        
        UserOn(l.EntityId)
            .Subscribe(e =>
            {
                _logger.LogDebug("Light Event: lights {Light} is in {State} by user {User}",  
                    e.New?.EntityId,
                    e.New?.State,
                    e.New?.Context?.UserId);
                lightFsm.FireOn();
                lightFsm.Timer.StartTimer(_config.SwitchTimer, lightFsm.Light.TurnOff);
            });
        UserOff(l.EntityId)
            .Subscribe(e =>
                {
                    _logger.LogDebug("Light Event: lights {Light} is in {State} by user {User}",  
                        e.New?.EntityId,
                        e.New?.State,
                        e.New?.Context?.UserId);
                    if (OnLights == 0) _fsmList.FireAllOff();
                    else lightFsm.FireOff();
                }
            );
        return lightFsm;
    }
    
    private IObservable<StateChange> LightEvent(string id) => _context.StateAllChanges()
        .Where(e => id == e.New?.EntityId);
    
    private IObservable<StateChange> UserEvent(string id) => LightEvent(id)
        .Where(e => !e.IsAutomationInitiated(_config.ServiceAccountId));
    
    private IObservable<StateChange> UserOn(string id) => UserEvent(id)
        .Where(e => e.New?.State == "on");
    
    private IObservable<StateChange> UserOff(string id) => UserEvent(id)
        .Where(e => e.New?.State == "off");
    
    private IObservable<StateChange> AutomationEvent(string id) => LightEvent(id)
        .Where(e => e.IsAutomationInitiated(_config.ServiceAccountId));

    private IObservable<StateChange> AutomationOn(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "on");
    
    private IObservable<StateChange> AutomationOff(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "off");
}
