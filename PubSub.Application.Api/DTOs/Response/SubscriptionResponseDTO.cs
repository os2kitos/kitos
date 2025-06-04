namespace PubSub.Application.Api.DTOs.Response
{
    public class SubscriptionResponseDTO
    {
        public Guid Uuid { get; set; }
        public string CallbackUrl { get; set; }
        public string Topic { get; set; }

    }
}
