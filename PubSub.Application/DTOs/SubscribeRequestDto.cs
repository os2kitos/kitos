namespace PubSub.Application.DTOs
{
    public record SubscribeRequestDto
    {
        public required Uri Callback { get; set; }
        public required IEnumerable<string> Topics { get; set; }
    }
}
