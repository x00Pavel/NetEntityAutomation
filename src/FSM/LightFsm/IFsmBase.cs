using Microsoft.Extensions.Logging;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.FSM.LightFsm;

public abstract class IFsmBase
{
    protected ILogger Logger;
    public CustomTimer Timer;
    protected string StoragePath { get; init; }
    public bool IsEnabled { get; set; } = true;
    
    protected abstract void ConfigureFsm();
    public abstract void FireOn();
    public abstract void FireOff();
}
