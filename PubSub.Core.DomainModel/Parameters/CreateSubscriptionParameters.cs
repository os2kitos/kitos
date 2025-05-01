namespace PubSub.Core.DomainModel.Parameters
{
    public class CreateSubscriptionParameters
    {
        public CreateSubscriptionParameters(string callback, string topic)
        {
            Callback = callback;
            Topic = topic;
        }

        public string Callback { get; set; }
        public string Topic { get; set; }
    }

}
