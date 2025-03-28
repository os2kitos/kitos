using System.Text.Json;

namespace PubSub.Core.Models
{
    public record Publication(Topic Topic, JsonElement Payload);
}
