using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.Automations;

public abstract class BaseAutomation<TConfig> where TConfig : IAutomationConfig
{
    protected ILogger Logger;

    protected TConfig Config { get; init; }

    protected IHaContext HaContext { get; init; }

    protected BaseAutomation(
        ILogger logger,
        TConfig config,
        IHaContext haContext
        )
    {
        Logger = logger;
        Config = config;
        HaContext = haContext;
    }
}



public abstract class BaseAutomation: BaseAutomation<IAutomationConfig>
{


    protected BaseAutomation(
        ILogger logger,
        IAutomationConfig config,
        IHaContext haContext
        ) : base(logger, config, haContext)
    {
        
    }
}