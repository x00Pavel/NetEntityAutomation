using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.FSM.LightFsm;

namespace NetEntityAutomation.Room.Core;


public interface IRoomConfigV1: IRoom
{
    public string Name => GetType().Name;
    public ILogger Logger { get; set; }
    public IEnumerable<AutomationConfig> Entities { get; set; }
    public NightModeConfig? NightMode { get; set; }
    
}