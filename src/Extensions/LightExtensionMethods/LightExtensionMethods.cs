using System.Text.Json.Serialization;
using NetDaemon.HassModel.Entities;
using Newtonsoft.Json;

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
    
    public static void TurnOn(this ILightEntityCore target, LightParameters parameters)
    {
        target.CallService("turn_on", parameters);
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
    
    public static void Toggle(this ILightEntityCore target, double? transition = null, long? brightnessPct = null)
    {
        target.CallService("toggle", new LightParameters { Transition = transition, BrightnessPct = brightnessPct});
    }
    
    public static void TurnOn(this IEnumerable<ILightEntityCore> target, double? transition = null, long? brightnessPct = null)
    {
        target.CallService("turn_on", new LightParameters { Transition = transition, BrightnessPct = brightnessPct});
    }
    
    public static void Toggle(this ILightEntityCore target, LightParameters parameters)
    {
        target.CallService("toggle", parameters);
    }
    
    public static void TurnOn(this IEnumerable<ILightEntityCore> target, LightParameters parameters)
    {
        target.CallService("turn_on", parameters);
    }
    
    public static LightParameters? GetLightParameters(this ILightEntityCore target)
    {
        return JsonConvert.DeserializeObject<LightParameters>(target.HaContext.GetState(target.EntityId)?.Attributes?.ToString() ?? "");
    }
}