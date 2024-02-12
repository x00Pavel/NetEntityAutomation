using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods;
public record SunAttributes
{
    [JsonPropertyName("next_dawn")]
    public string? NextDawn { get; init; }

    [JsonPropertyName("next_dusk")]
    public string? NextDusk { get; init; }

    [JsonPropertyName("next_midnight")]
    public string? NextMidnight { get; init; }

    [JsonPropertyName("next_noon")]
    public string? NextNoon { get; init; }

    [JsonPropertyName("next_rising")]
    public string? NextRising { get; init; }

    [JsonPropertyName("next_setting")]
    public string? NextSetting { get; init; }

    [JsonPropertyName("elevation")]
    public double? Elevation { get; init; }

    [JsonPropertyName("azimuth")]
    public double? Azimuth { get; init; }

    [JsonPropertyName("rising")]
    public bool? Rising { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }
}