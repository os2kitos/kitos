using System.Text.Json.Serialization;

namespace PubSub.Application.DTOs
{
    public class ClaimResponseDTO
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
