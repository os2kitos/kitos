namespace Tests.PubSubTester.DTOs
{
    public class SubscriptionDTO
    {
        public Uri Callback { get; set; }
        public IEnumerable<string> Topics { get; set; }
    }
}
