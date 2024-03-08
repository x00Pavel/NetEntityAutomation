using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

public record LightParameters
{
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

    [JsonPropertyName("color")]
    public object? Color { get; init; }

    [JsonPropertyName("effect")]
    public string? Effect { get; init; }

    [JsonPropertyName("off_with_transition")]
    public bool? OffWithTransition { get; init; }

    [JsonPropertyName("off_brightness")]
    public double? OffBrightness { get; init; }
}