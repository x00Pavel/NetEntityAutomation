using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.ExtensionMethods;
using NetEntityAutomation.Room.Core;

namespace NetEntityAutomation.Room.Automations;

public class BlindAutomationBase : AutomationBase
{
    private ISunEntityCore Sun;
    private IEnumerable<ICoverEntityCore> Blinds { get; set; }

    public BlindAutomationBase(IHaContext haContext, AutomationConfig automation, ILogger roomConfigLogger): base(haContext, roomConfigLogger, automation)
    {
        Blinds = Config.Entities.OfType<ICoverEntityCore>() ?? [];
        Sun = Config.Entities.OfType<ISunEntityCore>().First();
        var coverEntityCores = Blinds as ICoverEntityCore[] ?? Blinds.ToArray();
        if (Config.StartAtTimeFunc == null)
        {
            Sun.AboveHorizon().Subscribe(_ => Blinds.OpenCover());
            Logger.LogDebug("Subscribed to sun above horizon event to open blinds");
        }

        else
        {
            var time = Config.StartAtTimeFunc.Invoke();
            DailyEventAtTime(time, coverEntityCores.OpenCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to open blinds", time);
        }


        if (Config.StopAtTimeFunc == null)
        {
            Sun.BelowHorizon().Subscribe(_ => Blinds.CloseCover());
            Logger.LogDebug("Subscribed to sun above horizon event to close blinds");
        }
        else
        {
            DailyEventAtTime(Config.StopAtTimeFunc.Invoke(), coverEntityCores.CloseCover);
            Logger.LogDebug("Subscribed to daily event at {Time} to close blinds", Config.StopAtTimeFunc.Invoke());
        }
    }
}