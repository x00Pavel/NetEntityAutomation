using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods.Attributes;
public record CoverSetCoverPositionParameters
{
    ///<summary>Target position.</summary>
    [JsonPropertyName("position")]
    public long? Position { get; init; }
}