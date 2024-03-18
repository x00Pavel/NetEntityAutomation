using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Fsm;
using NetEntityAutomation.Extensions.ExtensionMethods;

namespace NetEntityAutomation.Core.Automations;

/// <summary>
/// Class for managing blinds automations.
/// Currently it is used for opening and closing blinds based on sun position (default, above horizon or below horizon) or specific time.
/// </summary>
public class BlindAutomationBase : AutomationBase<ICoverEntityCore, BlindsFsm>
{
    public required ISunEntityCore Sun { get; set; }
    private int OpenBlinds => EntitiesList.Count(b => Context.GetState(b.EntityId)?.State == "open");
    

    private bool IsOpenTime()
    {
        return (StartAtTimeFunc == null, StopAtTimeFunc == null) switch
        {
            (true, true) => Sun.IsAboveHorizon(),
            (false, false) => UtilsMethods.NowInTimeRange(StartAtTimeFunc!(), StopAtTimeFunc!()),
            (true, false) => Sun.IsAboveHorizon() && UtilsMethods.NowAfterTime(StopAtTimeFunc!()),
            (false, true) => UtilsMethods.NowBeforeTime(StartAtTimeFunc!()) && Sun.IsAboveHorizon(),
        };
    }
    
    private BlindsStateActivateAction BlindsActivateActions(ICoverEntityCore blind)
    {
        return new BlindsStateActivateAction
        {
            OpenByAutomationAction = () => ChooseAction(IsOpenTime(), blind.OpenCover, blind.CloseCover),
            OpenByManualAction = () => ChooseAction(IsOpenTime(), blind.OpenCover, blind.CloseCover),
            Closed = () => ChooseAction(!IsOpenTime(), blind.CloseCover, blind.OpenCover),
            CloseManuallyAction = () => ChooseAction(!IsOpenTime(), blind.CloseCover, blind.OpenCover)
        };
    }
    
    protected override BlindsFsm ConfigureFsm(ICoverEntityCore blind)
    {   
        var fsm = new BlindsFsm(Logger, blind).Configure(BlindsActivateActions(blind));
        UserClose(blind.EntityId).Subscribe(_ => ChooseAction(OpenBlinds > 0, fsm.FireManualClose, FsmList.FireAllOff));
        UserOpen(blind.EntityId).Subscribe(_ => fsm.FireManualOpen());
        AutomationClose(blind.EntityId).Subscribe(_ =>ChooseAction(OpenBlinds > 0, fsm.FireAutomationClose,  fsm.FireAutomationClose));
        AutomationOpen(blind.EntityId).Subscribe(_ => fsm.FireAutomationOpen());
        return fsm;
    }

    public override void ConfigureAutomation()
    {
        CreateFsm();
        Logger.LogDebug("Configuring blind automation");
        if (StartAtTimeFunc == null)
        {
            Sun.AboveHorizon().Subscribe(_ => EntitiesList.OpenCover());
            Logger.LogDebug("Subscribed to sun above horizon event to open blinds");
        }
        else
        {
            var time = StartAtTimeFunc.Invoke();
            DailyEventAtTime(time, EntitiesList.OpenCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to open blinds", time);
        }
        
        if (StopAtTimeFunc == null)
        {
            Sun.BelowHorizon().Subscribe(_ => EntitiesList.CloseCover());
            Logger.LogDebug("Subscribed to sun below horizon event to close blinds");
        }
        else
        {
            DailyEventAtTime(StopAtTimeFunc.Invoke(), EntitiesList.CloseCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to close blinds", StopAtTimeFunc.Invoke());
        }
    }
    
    private IObservable<StateChange> UserClose(string id) => UserEvent(id).Where(e => e.New?.State == "closed");
    private IObservable<StateChange> UserOpen(string id) => UserEvent(id).Where(e => e.New?.State == "open");
    private IObservable<StateChange> AutomationOpen(string id) => AutomationEvent(id).Where(e => e.New?.State == "open");
    private IObservable<StateChange> AutomationClose(string id) => AutomationEvent(id).Where(e => e.New?.State == "close");
}