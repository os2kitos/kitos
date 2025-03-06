namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEvent
{
    public KitosEvent(IEvent eventBody, string topic)
    {
        Topic = topic;
        EventBody = eventBody;

    }
    public string Topic { get; }

    public IEvent EventBody { get; }

}