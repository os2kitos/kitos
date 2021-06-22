using System;

namespace Core.ApplicationServices.Model.Notification
{
    public class GlobalAdminNotification
    {
        public DateTime OccurredAt { get; }
        public int ResponsibleUserId { get; }
        public string Subject { get; }
        public GlobalAdminNotificationMessage Message { get; }

        public GlobalAdminNotification(DateTime occurredAt, int responsibleUserId, string subject, GlobalAdminNotificationMessage message)
        {
            OccurredAt = occurredAt;
            ResponsibleUserId = responsibleUserId;
            Subject = subject;
            Message = message;
        }
    }
}
