using NetEntityAutomation.Automations.LightAutomation;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Extensions.Mapping;

public static class Mapping
{
    public static Dictionary<Type, Type> FsmToAutomation { get; } = new()
    {
        { typeof(ToggleFsmState), typeof(ToggleLightAutomation) },
        { typeof(OnOffFsmState), typeof(OnOffLightAutomation) }
    };
}