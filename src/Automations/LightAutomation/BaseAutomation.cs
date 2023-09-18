using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Automations.LightAutomation;

public abstract class BaseAutomation<TFsmState> where TFsmState: struct, Enum
{
    protected ILogger Logger;

    protected IAutomationConfig<TFsmState> Config;

    protected IHaContext HaContext;

    protected BaseAutomation(
        ILogger logger,
        IAutomationConfig<TFsmState> config,
        IHaContext haContext
        )
    {
        Logger = logger;
        Config = config;
        HaContext = haContext;
    }

    protected abstract void InitFsmTransitions();
}