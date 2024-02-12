using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Room.Core;

public enum BlindsTrigger
{
    AutomationOpenTrigger,
    AutomationCloseTrigger,
    ManualOpenTrigger,
    ManualCloseTrigger,
    AllCloseTrigger,
    AllOpenTrigger
}

public class BlindsFsm : IFsmBase<LightState, BlindsTrigger>
{
    public ICoverEntityCore Blinds { get; set; }

    public BlindsFsm(AutomationConfig config, ILogger logger) : base(config, logger)
    {
        CreateFsm();
    }
    
}