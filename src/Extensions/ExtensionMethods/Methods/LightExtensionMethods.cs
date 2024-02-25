﻿using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;
using Newtonsoft.Json;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

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
        target.CallService("toggle", new LightParameters { Brightness = brightnessPct});
    }
    
    public static void TurnOn(this IEnumerable<ILightEntityCore> target, double? transition = null, long? brightnessPct = null)
    {
        target.CallService("turn_on", new LightParameters { Brightness = brightnessPct});
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
        return JsonConvert.DeserializeObject<LightParameters>(target.HaContext.GetState(target.EntityId)?.AttributesJson.ToString() ?? "{}");
    }
    
    public static IObservable<StateChange> OnEvent(this ILightEntityCore target)
    {
        return target.StateChange().Where(e => e.New?.State == "on");
    }
    
    public static IObservable<StateChange> OffEvent(this ILightEntityCore target)
    {
        return target.StateChange().Where(e => e.New?.State == "off");
    }
}