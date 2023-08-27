using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.LightExtensionMethods;

public static class LightExtensionMethods
{
    public static void Toggle(this ILightEntityCore target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this ILightEntityCore target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this ILightEntityCore target)
    {
        target.CallService("turn_on");
    }
    
    public static void Toggle(this IEnumerable<ILightEntityCore> target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this IEnumerable<ILightEntityCore> target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this IEnumerable<ILightEntityCore> target)
    {
        target.CallService("turn_on");
    }
}