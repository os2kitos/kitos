using PubSub.Core.DomainModel.Topics;
using System.Text.Json;

namespace PubSub.Core.DomainModel.Publications
{
    public record Publication(Topic Topic, JsonElement Payload);
}
