using NetDaemon.HassModel.Entities;

namespace LightExtensionMethods;

public static class LightExtensionMethods
{
    public static void Toggle(this ILightEntity target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this ILightEntity target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this ILightEntity target)
    {
        target.CallService("turn_on");
    }
    
    public static void Toggle(this IEnumerable<ILightEntity> target)
    {
        target.CallService("toggle");
    }
    
    public static void TurnOff(this IEnumerable<ILightEntity> target)
    {
        target.CallService("turn_off");
    }
    
    public static void TurnOn(this IEnumerable<ILightEntity> target)
    {
        target.CallService("turn_on");
    }
}