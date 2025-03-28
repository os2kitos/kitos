using System.Text;
using System.Text.Json;

namespace PubSub.Core.Services.Serializer
{
    public class JsonPayloadSerializer : IPayloadSerializer
    {
        public JsonElement Deserialize(byte[] bytes)
        {
            var payloadStr = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<JsonElement>(payloadStr);
        }

        public byte[] Serialize(JsonElement message)
        {
            var rawString = message.GetRawText();
            return Encoding.UTF8.GetBytes(rawString);
        }
    }
}
