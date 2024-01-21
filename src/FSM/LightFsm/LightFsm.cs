using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.LightExtensionMethods;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class LightFsm<TState, TTRigger> : BaseLightFsm<TState, TTRigger> where TTRigger : Enum where TState : Enum
{
    protected LightFsm(ILogger logger, IFsmConfig<TState> config, string storageFileName) : base(logger, config, storageFileName)
    {
    }

    private void TurnOnLights()
    {   
        Logger.LogInformation("Turning on lights");
        if (Config.NightMode is { IsEnabled: true, IsWorkingHours: true })
        {   
            Config.NightMode.Devices?.TurnOn(brightnessPct: Config.NightMode.NightModeBrightness, transition: Config.NightMode.Transition);
        }
        else
        {
            Config.Lights.TurnOn(brightnessPct: 100, transition: Config.Transition);    
        }
        
        Timer?.Dispose();
    }
    
    protected void TurnOnWithTimer(TimeSpan timer)
    {
        StartTimer(timer);
        TurnOnLights();
    }

    
    protected void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        Config.Lights.TurnOff();
        Timer?.Dispose();
    }
    
    public void MotionOn(TTRigger motionOnTrigger)
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
    
    public void MotionOff(TTRigger motionOffTrigger)
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