using NetDaemon.HassModel.Entities;

namespace LightExtensionMethods;

public static class LightExtensionMethods
{
    public static void Toggle(this IOnOffTarget target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this IOnOffTarget target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this IOnOffTarget target)
    {
        target.CallService("turn_on");
    }
    
    public static void Toggle(this IEnumerable<IOnOffTarget> target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this IEnumerable<IOnOffTarget> target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this IEnumerable<IOnOffTarget> target)
    {
        target.CallService("turn_on");
    }
}