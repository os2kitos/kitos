using System.Text.Json;

namespace PubSub.Core.Services.Serializer
{
    public interface IPayloadSerializer
    {
        byte[] Serialize(JsonElement message);

        JsonElement Deserialize(byte[] bytes);
    }
}
