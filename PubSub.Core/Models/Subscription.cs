namespace PubSub.Core.Models
{
    public class Subscription
    {
        public Subscription(string callback, string topic, string ownerId)
        {
            Callback = callback;
            Topic = topic;
            OwnerId = ownerId;
            Uuid = Guid.NewGuid();
        }
        public Guid Uuid { get; set; }
        public string Callback { get; set; }
        public string Topic { get; set; }
        public string OwnerId { get; set; }
    }
}
