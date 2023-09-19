using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.Automations.LightAutomation;



// dictionary with maping of FSM state to automation class
// static Dictionary<string, string> fsmToAutomationMap = new Dictionary<string, string>();

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