using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.LightExtensionMethods;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class LightFsm<TState, TTRigger> : BaseFsm<TState, TTRigger> where TTRigger : Enum where TState : Enum
{
    protected LightFsm(ILogger logger, IFsmConfig<TState> config, string storagePath) : base(logger, config, storagePath)
    {
    }

    protected void TurnOnLights()
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

    protected void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        Config.Lights.TurnOff();
        Timer?.Dispose();
    }
}