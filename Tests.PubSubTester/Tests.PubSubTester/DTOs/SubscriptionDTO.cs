namespace Tests.PubSubTester.DTOs
{
    public class SubscriptionDTO
    {
        public string Callback { get; set; }
        public List<string> Topics { get; set; }
    }
}
