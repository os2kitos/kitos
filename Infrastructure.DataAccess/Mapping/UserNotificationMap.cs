using Core.DomainModel.Notification;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserNotificationMap : EntityMap<UserNotification>
    {
        public override void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            base.Configure(builder);
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property(x => x.NotificationMessage)
                .IsRequired();

            builder.Property(x => x.NotificationType)
                .IsRequired();

            builder.HasOne(x => x.Organization)
                .WithMany(x => x.UserNotifications);

            builder.HasIndex(x => x.NotificationRecipientId);

            builder.HasOne(x => x.ItContract)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.Itcontract_Id);

            builder.HasOne(x => x.ItSystemUsage)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.ItSystemUsage_Id);

            builder.HasOne(x => x.DataProcessingRegistration)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.DataProcessingRegistration_Id);
        }
    }
}
