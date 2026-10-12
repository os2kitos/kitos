namespace PubSub.Application.Api.DTOs.Response
{
    public class SubscriptionResponseDTO
    {
        public Guid Uuid { get; set; }
        public required string CallbackUrl { get; set; }
        public required string Topic { get; set; }

    }
}
