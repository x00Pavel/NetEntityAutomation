using Microsoft.Extensions.Logging;

namespace NetEntityAutomation.Core.Configs;

public interface IRoom;

public interface IRoomConfigV1 : IRoom
{
    public string Name => GetType().Name;
    public ILogger Logger { get; set; }
    public IEnumerable<AutomationConfig> Entities { get; set; }
    public NightModeConfig? NightMode { get; set; }
}