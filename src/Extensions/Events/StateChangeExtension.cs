using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.Events;

public static class StateChangeExtension
{
    public static bool IsAutomationInitiated(this StateChange stateChange, string serviceUser)
    {
        var userId = stateChange.Entity.EntityState?.Context?.UserId ?? "no-user";
        return userId == serviceUser;
    }
}