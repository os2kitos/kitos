namespace Core.ApplicationServices.Model.KitosEvents;

public interface IKitosEventMapper
{
    KitosEventDTO MapKitosEventToDTO(KitosEvent kitosEvent);
}