using Newtonsoft.Json;

namespace Core.ApplicationServices.Model.KitosEvents;

public class KitosEventMapper : IKitosEventMapper
{
    public KitosEventDTO MapKitosEventToDTO(KitosEvent kitosEvent)
    {
        var message = JsonConvert.SerializeObject(kitosEvent.EventBody.ToKeyValuePairs());
        var topic = kitosEvent.Topic;
        return new KitosEventDTO(message, topic);
    }
}