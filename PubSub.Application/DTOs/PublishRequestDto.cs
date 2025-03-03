namespace PubSub.Application.DTOs
{
    public class PublishRequestDto
    {
        public required string Topic { get; set; }
        public required string Message { get; set; }
    }
}
