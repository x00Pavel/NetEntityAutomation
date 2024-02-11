using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public enum SimpleState
{
    OffState,
    OnState,
    UnavailableState,
}

public enum SimpleTrigger
{
    TurnOn,
    TurnOff,
    Unavailable
}

public class SimpleFsm: IFsmBase
{
    private SimpleState _state;
    private readonly StateMachine<SimpleState, SimpleTrigger> Fsm;
    public SimpleFsm()
    {
        Fsm = new StateMachine<SimpleState, SimpleTrigger>(SimpleState.OffState);
        ConfigureFsm();
    }
    
    protected sealed override void ConfigureFsm()
    {
        Fsm.Configure(SimpleState.OffState)
            .Permit(SimpleTrigger.TurnOn, SimpleState.OnState)
            .Permit(SimpleTrigger.Unavailable, SimpleState.UnavailableState);
        Fsm.Configure(SimpleState.OnState)
            .Permit(SimpleTrigger.TurnOff, SimpleState.OffState)
            .Permit(SimpleTrigger.Unavailable, SimpleState.UnavailableState);
        Fsm.Configure(SimpleState.UnavailableState)
            .Permit(SimpleTrigger.TurnOff, SimpleState.OffState)
            .Permit(SimpleTrigger.TurnOn, SimpleState.OnState);
    }
    
    public override void FireOn()
    {
        Fsm.Fire(SimpleTrigger.TurnOn);
    }

    public override void FireOff()
    {
        throw new NotImplementedException();
    }
}