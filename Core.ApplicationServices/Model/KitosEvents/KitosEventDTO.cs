namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEventDTO
{
    public KitosEventDTO(object payload, string topic)
    {

        Payload = payload;
        Topic = topic;
    }
    public object Payload { get; }
    public string Topic { get; }
}