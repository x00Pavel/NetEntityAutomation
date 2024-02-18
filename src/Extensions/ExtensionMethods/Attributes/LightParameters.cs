using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

public record LightParameters
{
    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }

    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    [JsonPropertyName("min_color_temp_kelvin")]
    public double? MinColorTempKelvin { get; init; }

    [JsonPropertyName("max_color_temp_kelvin")]
    public double? MaxColorTempKelvin { get; init; }

    [JsonPropertyName("min_mireds")]
    public double? MinMireds { get; init; }

    [JsonPropertyName("max_mireds")]
    public double? MaxMireds { get; init; }

    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }

    [JsonPropertyName("color_temp_kelvin")]
    public double? ColorTempKelvin { get; init; }

    [JsonPropertyName("color_temp")]
    public double? ColorTemp { get; init; }

    [JsonPropertyName("hs_color")]
    public IReadOnlyList<double>? HsColor { get; init; }

    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<double>? RgbColor { get; init; }

    [JsonPropertyName("xy_color")]
    public IReadOnlyList<double>? XyColor { get; init; }

    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    [JsonPropertyName("effect_list")]
    public IReadOnlyList<string>? EffectList { get; init; }

    [JsonPropertyName("color")]
    public object? Color { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("effect")]
    public string? Effect { get; init; }

    [JsonPropertyName("off_with_transition")]
    public bool? OffWithTransition { get; init; }

    [JsonPropertyName("off_brightness")]
    public double? OffBrightness { get; init; }

    [JsonPropertyName("flowing")]
    public object? Flowing { get; init; }

    [JsonPropertyName("music_mode")]
    public bool? MusicMode { get; init; }
}