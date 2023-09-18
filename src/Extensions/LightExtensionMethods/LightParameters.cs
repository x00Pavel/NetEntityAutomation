using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.LightExtensionMethods;

public record LightParameters
{
    [JsonPropertyName("transition")]
    public long? Transition { get; init; }
    
    [JsonPropertyName("brightness_pct")]
    public long? BrightnessPct { get; init; }
}