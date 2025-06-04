using System.Text.Json;

namespace PubSub.Application.Api.DTOs.Request
{
    public class PublishRequestDto
    {
        public required string Topic { get; set; }
        public required JsonElement Payload { get; set; }
    }
}
