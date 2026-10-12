using System.Text.Json.Serialization;

namespace PubSub.Application.Api.DTOs.Response
{
    public class ClaimResponseDTO
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }
        [JsonPropertyName("value")]
        public required string Value { get; set; }
    }
}
