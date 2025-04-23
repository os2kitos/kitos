namespace PubSub.Application.DTOs
{
    public class SubscriptionResponseDTO
    {
        public Guid Uuid { get; set; }
        public string CallbackUrl { get; set; }
        public string Topic { get; set; }

    }
}
