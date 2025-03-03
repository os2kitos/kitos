namespace PubSub.Core.Services.Serializer
{
    public interface IMessageSerializer
    {
        byte[] Serialize(string message);

        string Deserialize(byte[] bytes);
    }
}
