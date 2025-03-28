namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEventMapper : IKitosEventMapper
{
    public KitosEventDTO MapKitosEventToDTO(KitosEvent kitosEvent)
    {
        var payload = kitosEvent.EventBody.ToKeyValuePairs();
        var topic = kitosEvent.Topic;
        return new KitosEventDTO(payload, topic);
    }
}