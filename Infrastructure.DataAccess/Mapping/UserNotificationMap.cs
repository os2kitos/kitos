using Core.DomainModel.Notification;
using System.Data.Entity.ModelConfiguration;

namespace Infrastructure.DataAccess.Mapping
{
    public class UserNotificationMap : EntityTypeConfiguration<UserNotification>
    {
        public UserNotificationMap()
        {
            HasKey(x => x.Id);

            Property(x => x.Name)
                .HasMaxLength(UserNotification.MaxNameLength)
                .IsRequired();

            Property(x => x.NotificationMessage)
                .HasMaxLength(UserNotification.MaxMessageLength)
                .IsRequired();

            Property(x => x.RelatedEntityId)
                .IsRequired();

            Property(x => x.RelatedEntityType)
                .IsRequired();
        }
    }
}
