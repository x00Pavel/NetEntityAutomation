using Microsoft.Extensions.Logging;
using NetEntityAutomation.Core.Automations;
using NetEntityAutomation.Core.Configs;

namespace NetEntityAutomation.Core.RoomManager;

public interface IRoom;

/// <summary>
/// This class is primary used by user to provide configuration for a room.
/// </summary>
public interface IRoomConfig : IRoom
{
    /// <summary>
    /// Name of the room.
    /// </summary>
    public string Name => GetType().Name;
    /// <summary>
    /// Logger to be used by the room.
    /// This is externally useful for debugging and logging.
    /// </summary>
    public ILogger Logger { get; set; }
    /// <summary>
    /// A list of automation configurations that will be used by the room.
    /// See <see cref="IAutomationConfig"/> for more details.
    /// </summary>
    public IEnumerable<AutomationBase> Entities { get; set; }
    
    /// <summary>
    /// Night mode for the room.
    /// <remarks>
    /// Currently this attribute is not used.
    /// It is planned to be used as default value for all automations in the room if the night mode is not explicitly set in the automation configuration.
    /// </remarks>
    /// </summary>
    public NightModeConfig? NightMode { get; set; }
}