namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEvent
{
    public KitosEvent(IEventBody eventBody, string topic)
    {
        Topic = topic;
        EventBody = eventBody;

    }
    public string Topic { get; }

    public IEventBody EventBody { get; }

}