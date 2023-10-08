using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetEntityAutomation.Automations;

namespace StatefulAutomation;

public abstract class StatefulAutomation<TConfig, TFsmState> : BaseAutomation<TConfig>
    where TConfig : IStatefulAutomationConfig<TFsmState> where TFsmState : Enum
{
    protected StatefulAutomation(
        ILogger logger,
        TConfig config,
        IHaContext haContext
        ) : base(logger, config, haContext)
    {
        InitFsmTransitions();
    }

    protected virtual void InitFsmTransitions()
    {
        throw new NotImplementedException();
    }
}