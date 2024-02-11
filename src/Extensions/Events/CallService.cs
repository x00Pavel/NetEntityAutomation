using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.Events;

public record CallService
{
    [JsonPropertyName("domain")] public string? Domain { get; init; }
    [JsonPropertyName("service")] public string? Service { get; init; }
    [JsonPropertyName("service_data")] public JsonElement? ServiceData { get; init; }
    
    // TODO: catch exception if property does not exist
    public JsonElement GetProperty (string property) => ServiceData.GetValueOrDefault().GetProperty(property);
}