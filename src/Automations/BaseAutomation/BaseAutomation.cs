using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.Automations;

public abstract class BaseAutomation
{
    protected ILogger Logger;

    protected IAutomationConfig Config { get; init; }

    protected IHaContext HaContext { get; init; }

    protected BaseAutomation(
        ILogger logger,
        IAutomationConfig config,
        IHaContext haContext
        )
    {
        Logger = logger;
        Config = config;
        HaContext = haContext;
    }

    protected abstract void InitFsmTransitions();
}