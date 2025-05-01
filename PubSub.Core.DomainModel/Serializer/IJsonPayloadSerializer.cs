using System.Text.Json;

namespace PubSub.Core.DomainModel.Serializer
{
    public interface IJsonPayloadSerializer
    {
        byte[] Serialize(JsonElement message);

        JsonElement Deserialize(byte[] bytes);
    }
}
