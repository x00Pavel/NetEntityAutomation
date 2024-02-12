using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

public record LightParameters
{
    [JsonPropertyName("transition")]
    public double? Transition { get; init; }
    
    [JsonPropertyName("brightness_pct")]
    public long? BrightnessPct { get; init; }
}