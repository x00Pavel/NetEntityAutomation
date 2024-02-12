using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;

namespace NetEntityAutomation.Room.Core;

public abstract class AutomationBase
{
    protected IHaContext Context { get; set; }
    protected ILogger Logger { get; set; }
    protected AutomationConfig Config { get; set; }

    protected void DailyEventAtTime(TimeSpan timeSpan, Action action)
    {
        var triggerIn = timeSpan - DateTime.Now.TimeOfDay;
        Observable.Timer(triggerIn, TimeSpan.FromDays(1)).Subscribe(e =>
        {
            Logger.LogDebug("Daily event at {Time} triggered", timeSpan);
            action();
        });
        Logger.LogDebug("Triggering first event in {Time}", triggerIn);
    }
}