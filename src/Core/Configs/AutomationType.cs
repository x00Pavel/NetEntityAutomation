namespace NetEntityAutomation.Core.Configs;

/// <summary>
/// Supported types of automation.
/// <list type="bullet">
///     <item>
///         <term>MainLight</term>
///     </item>
///     <item>
///         <term>SecondaryLight</term>
///     </item>
///     <item>
///         <term>Blinds</term>
///     </item>
/// </list>
/// </summary>
public enum AutomationType
{
    MainLight,
    SecondaryLight,
    Blinds,
}