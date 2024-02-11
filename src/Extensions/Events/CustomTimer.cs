using System.Reactive.Linq;
using Microsoft.Extensions.Logging;

namespace NetEntityAutomation.Extensions.Events;

public class CustomTimer(ILogger logger): IDisposable
{
    private IDisposable? _timer;
    private ILogger Logger { get; } = logger;

    // Create the constructor that take a function as a parameter with no argmunents and void return type

    public void StartTimer(TimeSpan waitTime, Action action)
    {
        Logger.LogDebug("Starting timer for {WaitHours}:{WaitMinutes}:{WaitTime}", waitTime.Hours, waitTime.Minutes, waitTime.Seconds);
        _timer?.Dispose();
        _timer = Observable.Timer(waitTime)
            .Subscribe(_ =>
            {
                Logger.LogDebug("Calling action");
                action();
            });
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}