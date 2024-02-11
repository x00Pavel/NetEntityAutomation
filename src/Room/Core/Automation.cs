using NetDaemon.HassModel;
using NetEntityAutomation.FSM.LightFsm;
using NetEntityAutomation.Room.Interfaces;

namespace NetEntityAutomation.Room.Core;
/// <summary>
/// Automation is responsible for connecting entities, triggers and actions to our flow.
/// To do this we use a Finite State Machine (FSM) to keep track of the state of the entities.
/// </summary>
public class Automation
{   
    private IHaContext HaContext { get; set; }
    private IFsmBase Fsm { get; set; }
    private AutomationConfig Config { get; set; }

    public Automation(AutomationConfig config, IHaContext haContext)
    {
        HaContext = haContext;
        Config = config;

        switch (config.AutomationType)
        {
            case AutomationType.MainLight:
                
                break;
            case AutomationType.SecondaryLight:
                break;
            case AutomationType.Blinds:
                break;
            default:
                break;
        }
        
        // Fsm = config.AutomationType switch
        // {
        //     AutomationType.MainLight => new SimpleFsm(),
        //     AutomationType.SecondaryLight => new SimpleFsm(),
        //     AutomationType.Blinds => new SimpleFsm(),
        //     _ => throw new ArgumentOutOfRangeException()
        // };
        // foreach (var t in Config.Triggers)
        // {
        //     t.HaContext = haContext;
        //     t.ConfigureFsmTransition(Fsm);
        // }
        //
        
    }
}