using Microsoft.Extensions.Logging;
using NetEntityAutomation.Extensions.LightExtensionMethods;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class LightFsm<TState, TTRigger> : BaseFsm<TState, TTRigger> where TTRigger : Enum where TState : Enum
{
    protected LightFsm(ILogger logger, FsmConfig<TState> config) : base(logger, config)
    {
    }

    protected void TurnOnLights()
    {
        Logger.LogInformation("Turning on lights");
        Config.Lights.TurnOn();
        Timer?.Dispose();
    }

    protected void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        Config.Lights.TurnOff();
        Timer?.Dispose();
    }
}