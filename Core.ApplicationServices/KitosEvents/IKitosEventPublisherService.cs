using Core.ApplicationServices.Model.KitosEvents;

namespace Core.ApplicationServices.KitosEvents;

public interface IKitosEventPublisherService
{
    void PublishEvent(KitosEvent kitosEvent);
}