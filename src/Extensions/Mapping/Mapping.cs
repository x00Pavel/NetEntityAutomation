using NetEntityAutomation.Automations.LightAutomation;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Extesions.Mapping;

public static class Mapping
{
    public static Dictionary<Type, Type> FsmToAutomation { get; } = new()
    {
        { typeof(ToggleFsmState), typeof(ToggleLightAutomation) },
        { typeof(OnOffFsmState), typeof(OnOffLightAutomation) }
    };
}