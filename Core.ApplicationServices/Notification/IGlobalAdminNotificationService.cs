using Core.ApplicationServices.Model.Notification;

namespace Core.ApplicationServices.Notification
{
    public interface IGlobalAdminNotificationService
    {
        void Submit(GlobalAdminNotification notification);
    }
}
