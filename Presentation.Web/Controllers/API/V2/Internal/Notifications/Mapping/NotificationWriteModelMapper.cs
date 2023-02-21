using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Core.ApplicationServices.Model.Notification.Write;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationWriteModelMapper : INotificationWriteModelMapper
    {
        public ImmediateNotificationModificationParameters FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapImmediateNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        public ScheduledNotificationModificationParameters FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapScheduledNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        public UpdateScheduledNotificationModificationParameters FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapUpdateScheduledNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        private ImmediateNotificationModificationParameters MapImmediateNotificationWriteRequestDTO(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return new ImmediateNotificationModificationParameters
            (
                dto.Body,
                dto.Subject,
                ownerResourceType.ToRelatedEntityType(),
                dto.OwnerResourceUuid,
                MapRootRecipients(dto.Ccs),
                MapRootRecipients(dto.Receivers)
            );
        }

        private UpdateScheduledNotificationModificationParameters MapUpdateScheduledNotificationWriteRequestDTO(UpdateScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return new UpdateScheduledNotificationModificationParameters
            (
                dto.Body,
                dto.Subject,
                ownerResourceType.ToRelatedEntityType(),
                dto.OwnerResourceUuid,
                MapRootRecipients(dto.Ccs),
                MapRootRecipients(dto.Receivers),
                dto.Name,
                dto.ToDate
            );
        }

        private ScheduledNotificationModificationParameters MapScheduledNotificationWriteRequestDTO(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return new ScheduledNotificationModificationParameters
                (
                    dto.Body,
                    dto.Subject,
                    ownerResourceType.ToRelatedEntityType(),
                    dto.OwnerResourceUuid,
                    MapRootRecipients(dto.Ccs),
                    MapRootRecipients(dto.Receivers),
                    dto.Name,
                    dto.ToDate,
                    dto.RepetitionFrequency.ToScheduling(),
                    dto.FromDate
                );
        }

        private RootRecipientModificationParameters MapRootRecipients(RecipientWriteRequestDTO dto)
        {
            return dto != null ? new RootRecipientModificationParameters
            (
                MapEmailRecipients(dto.EmailRecipients),
                MapRoleRecipients(dto.RoleRecipients)
            ) : null;
        }

        private static IEnumerable<EmailRecipientModificationParameters> MapEmailRecipients(IEnumerable<EmailRecipientWriteRequestDTO> dtos)
        {
            return (dtos ?? new List<EmailRecipientWriteRequestDTO>()).Select(x => new EmailRecipientModificationParameters(x.Email)).ToList();
        }

        public IEnumerable<RoleRecipientModificationParameters> MapRoleRecipients(IEnumerable<RoleRecipientWriteRequestDTO> dtos)
        {
            return (dtos ?? new List<RoleRecipientWriteRequestDTO>()).Select(x => new RoleRecipientModificationParameters(x.RoleUuid)).ToList();
        }
    }
}