using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class LightFsm<TState, TTrigger>(ILogger logger, IFsmConfig<TState> config, string storageFileName)
    : BaseLightFsm<TState, TTrigger>(logger, config, storageFileName)
    where TTrigger : Enum
    where TState : Enum
{
    protected void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        Timer?.Dispose();
    }
    
    public void MotionOn(TTrigger motionOnTrigger)
    {
        try
        {
            Logger.LogInformation("Motion on");
            StateMachine.Fire(motionOnTrigger);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
    
    public void MotionOff(TTrigger motionOffTrigger)
    {
        try
        {
            Logger.LogInformation("Motion off");
            StateMachine.Fire(motionOffTrigger);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
}