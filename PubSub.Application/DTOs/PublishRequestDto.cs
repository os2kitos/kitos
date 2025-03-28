using System.Text.Json;

namespace PubSub.Application.DTOs
{
    public class PublishRequestDto
    {
        public required string Topic { get; set; }
        public required JsonElement Payload { get; set; }
    }
}
