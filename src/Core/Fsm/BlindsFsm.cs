using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Configs;

namespace NetEntityAutomation.Core.Fsm;

public enum BlindsTrigger
{
    AutomationOpenTrigger,
    AutomationCloseTrigger,
    ManualOpenTrigger,
    ManualCloseTrigger,
    AllCloseTrigger,
    AllOpenTrigger
}

public enum BlindsState
{
    OpenByAutomation,
    OpenByManual,
    Closed,
    CloseManually
}

public struct BlindsStateActivateAction
{
    public Action OpenByAutomationAction { get; init; }
    public Action OpenByManualAction { get; init; }
    public Action Closed { get; init; }
    public Action CloseManuallyAction { get; init; }
}

public class BlindsFsm : FsmBase<BlindsState, BlindsTrigger>
{
    private ICoverEntityCore Blinds { get; set; }

    public BlindsFsm(AutomationConfig config, ILogger logger, ICoverEntityCore blinds) : base(config, logger)
    {
        Logger = logger;
        Blinds = blinds;
        DefaultState = BlindsState.Closed;
        StoragePath = $"storage/v1/{Blinds.EntityId}_fsm.json";
        InitFsm();
    }
    
    public void FireAutomationOpen() => _fsm.Fire(BlindsTrigger.AutomationOpenTrigger);
    public void FireAutomationClose() => _fsm.Fire(BlindsTrigger.AutomationCloseTrigger);
    public void FireManualOpen() => _fsm.Fire(BlindsTrigger.ManualOpenTrigger);
    public void FireManualClose() => _fsm.Fire(BlindsTrigger.ManualCloseTrigger);
    public void FireAllOpen() => _fsm.Fire(BlindsTrigger.AllOpenTrigger);

    public override void FireAllOff() => _fsm.Fire(BlindsTrigger.AllCloseTrigger);
    public BlindsFsm Configure(BlindsStateActivateAction blindsStateActions)
    {
        _fsm.Configure(BlindsState.Closed)
            .OnActivate(blindsStateActions.Closed)
            .PermitReentry(BlindsTrigger.AllCloseTrigger)
            .PermitReentry(BlindsTrigger.AutomationCloseTrigger)
            .Permit(BlindsTrigger.AllOpenTrigger, BlindsState.OpenByAutomation)
            .Permit(BlindsTrigger.AutomationOpenTrigger, BlindsState.OpenByAutomation)
            .Permit(BlindsTrigger.ManualOpenTrigger, BlindsState.OpenByManual);

        _fsm.Configure(BlindsState.OpenByAutomation)
            .OnActivate(blindsStateActions.OpenByAutomationAction)
            .Ignore(BlindsTrigger.ManualOpenTrigger)
            .PermitReentry(BlindsTrigger.AllOpenTrigger)
            .PermitReentry(BlindsTrigger.AutomationOpenTrigger)
            .Permit(BlindsTrigger.AllCloseTrigger, BlindsState.Closed)
            .Permit(BlindsTrigger.AutomationCloseTrigger, BlindsState.Closed)
            .Permit(BlindsTrigger.ManualCloseTrigger, BlindsState.CloseManually);

        _fsm.Configure(BlindsState.OpenByManual)
            .OnActivate(blindsStateActions.OpenByManualAction)
            .PermitReentry(BlindsTrigger.ManualOpenTrigger)
            .Ignore(BlindsTrigger.AllOpenTrigger)
            .Ignore(BlindsTrigger.AutomationCloseTrigger)
            .Ignore(BlindsTrigger.AutomationOpenTrigger)
            .Permit(BlindsTrigger.ManualCloseTrigger, BlindsState.CloseManually)
            .Permit(BlindsTrigger.AllCloseTrigger, BlindsState.Closed);
        
        _fsm.Configure(BlindsState.CloseManually)
            .OnActivate(blindsStateActions.CloseManuallyAction)
            .PermitReentry(BlindsTrigger.ManualCloseTrigger)
            .Ignore(BlindsTrigger.AutomationCloseTrigger)
            .Ignore(BlindsTrigger.AutomationOpenTrigger)
            .Ignore(BlindsTrigger.AllOpenTrigger)
            .Permit(BlindsTrigger.AllCloseTrigger, BlindsState.Closed)
            .Permit(BlindsTrigger.ManualOpenTrigger, BlindsState.OpenByManual);
        return this;
    }
}