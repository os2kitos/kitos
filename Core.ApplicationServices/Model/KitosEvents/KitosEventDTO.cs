namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEventDTO
{
    public KitosEventDTO(string message, string topic)
    {

        Message = message;
        Topic = topic;
    }
    public string Message { get; }
    public string Topic { get; }
}