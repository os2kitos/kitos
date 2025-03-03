using System.Text;

namespace PubSub.Core.Services.Serializer
{
    public class UTF8MessageSerializer : IMessageSerializer
    {
        public string Deserialize(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] Serialize(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }
    }
}
