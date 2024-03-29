﻿using Core.DomainModel.Notification;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserNotificationMap : EntityMap<UserNotification>
    {
        public UserNotificationMap()
        {
            HasKey(x => x.Id);

            Property(x => x.Name)
                .IsRequired();

            Property(x => x.NotificationMessage)
                .IsRequired();

            Property(x => x.NotificationType)
                .IsRequired();

            HasRequired(x => x.Organization)
                .WithMany(x => x.UserNotifications);

            HasIndex(x => x.NotificationRecipientId);

            HasOptional(x => x.ItContract)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.Itcontract_Id);

            HasOptional(x => x.ItSystemUsage)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.ItSystemUsage_Id);

            HasOptional(x => x.DataProcessingRegistration)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.DataProcessingRegistration_Id);
        }
    }
}
