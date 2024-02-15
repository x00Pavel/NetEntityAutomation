using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.ExtensionMethods;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Core.Fsm;

namespace NetEntityAutomation.Core.Automations;

/// <summary>
/// Class for managing blinds automations.
/// Currently it is used for opening and closing blinds based on sun position (default, above horizon or below horizon) or specific time.
/// </summary>
public class BlindAutomationBase : AutomationBase<ICoverEntityCore, BlindsFsm>
{
    private readonly ISunEntityCore _sun;
    private int OpenBlinds => EntitiesList.Count(b => Context.GetState(b.EntityId)?.State == "open");

    public BlindAutomationBase(IHaContext haContext, AutomationConfig automation, ILogger roomConfigLogger): base(haContext, automation, roomConfigLogger)
    {
        _sun = Config.Entities.OfType<ISunEntityCore>().First();
        CreateFsm();
        ConfigureAutomation();
    }

    protected override BlindsFsm ConfigureFsm(ICoverEntityCore blind)
    {   
        var fsm = new BlindsFsm(Config, Logger, blind);
        UserClose(blind.EntityId).Subscribe(_ => ChooseAction(OpenBlinds > 0, fsm.FireManualClose, FsmList.FireAllOff));
        UserOpen(blind.EntityId).Subscribe(_ => fsm.FireManualOpen());
        AutomationClose(blind.EntityId).Subscribe(_ =>ChooseAction(OpenBlinds > 0, fsm.FireAutomationClose,  fsm.FireAutomationClose));
        AutomationOpen(blind.EntityId).Subscribe(_ => fsm.FireAutomationOpen());
        return fsm;
    }

    private void ConfigureAutomation()
    {   
        Logger.LogDebug("Configuring blind automation");
        if (Config.StartAtTimeFunc == null)
        {
            _sun.AboveHorizon().Subscribe(_ => EntitiesList.OpenCover());
            Logger.LogDebug("Subscribed to sun above horizon event to open blinds");
        }
        else
        {
            var time = Config.StartAtTimeFunc.Invoke();
            DailyEventAtTime(time, EntitiesList.OpenCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to open blinds", time);
        }
        
        if (Config.StopAtTimeFunc == null)
        {
            _sun.BelowHorizon().Subscribe(_ => EntitiesList.CloseCover());
            Logger.LogDebug("Subscribed to sun below horizon event to close blinds");
        }
        else
        {
            DailyEventAtTime(Config.StopAtTimeFunc.Invoke(), EntitiesList.CloseCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to close blinds", Config.StopAtTimeFunc.Invoke());
        }
    }
    
    private IObservable<StateChange> UserClose(string id) => UserEvent(id).Where(e => e.New?.State == "closed");
    private IObservable<StateChange> UserOpen(string id) => UserEvent(id).Where(e => e.New?.State == "open");
    private IObservable<StateChange> AutomationOpen(string id) => AutomationEvent(id).Where(e => e.New?.State == "open");
    private IObservable<StateChange> AutomationClose(string id) => AutomationEvent(id).Where(e => e.New?.State == "close");
}