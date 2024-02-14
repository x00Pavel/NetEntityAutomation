using System.Text.Json.Serialization;

namespace NetEntityAutomation.Extensions.ExtensionMethods.Attributes;
public record CoverSetCoverTiltPositionParameters
{
    ///<summary>Target tilt positition.</summary>
    [JsonPropertyName("tilt_position")]
    public long? TiltPosition { get; init; }
}