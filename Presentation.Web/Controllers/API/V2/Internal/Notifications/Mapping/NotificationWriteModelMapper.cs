using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Types.Notifications;
using Core.ApplicationServices.Model.Notification.Write;
using System;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationWriteModelMapper : INotificationWriteModelMapper
    {
        public ImmediateNotificationModificationParameters FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return MapImmediateNotificationWriteRequestDTO(dto, ownerResourceUuid, ownerResourceType);
        }

        public CreateScheduledNotificationModificationParameters FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return MapScheduledNotificationWriteRequestDTO(dto, ownerResourceUuid, ownerResourceType);
        }

        public UpdateScheduledNotificationModificationParameters FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return MapUpdateScheduledNotificationWriteRequestDTO(dto, ownerResourceUuid, ownerResourceType);
        }

        private ImmediateNotificationModificationParameters MapImmediateNotificationWriteRequestDTO(ImmediateNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return new ImmediateNotificationModificationParameters
            (
                MapBaseProperties(dto, ownerResourceUuid, ownerResourceType)
            );
        }

        private UpdateScheduledNotificationModificationParameters MapUpdateScheduledNotificationWriteRequestDTO(UpdateScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return new UpdateScheduledNotificationModificationParameters
            (
                MapBaseProperties(dto, ownerResourceUuid, ownerResourceType),
                dto.Name, dto.ToDate
            );
        }

        private CreateScheduledNotificationModificationParameters MapScheduledNotificationWriteRequestDTO(ScheduledNotificationWriteRequestDTO dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
        {
            return new CreateScheduledNotificationModificationParameters
            (
                MapBaseProperties(dto, ownerResourceUuid, ownerResourceType),
                dto.Name, 
                dto.ToDate, 
                dto.RepetitionFrequency.ToScheduling(),
                dto.FromDate
            );
        }

        private BaseNotificationPropertiesModificationParameters MapBaseProperties<T>(T dto, Guid ownerResourceUuid, OwnerResourceType ownerResourceType)
            where T : class, IHasBaseWriteProperties
        {
            return new BaseNotificationPropertiesModificationParameters(dto.BaseProperties.Body,
                dto.BaseProperties.Subject,
                ownerResourceType.ToRelatedEntityType(),
                ownerResourceUuid,
                MapRootRecipients(dto.BaseProperties.Ccs),
                MapRootRecipients(dto.BaseProperties.Receivers));
        }

        private RootRecipientModificationParameters MapRootRecipients(RecipientWriteRequestDTO dto)
        {
            return dto != null ? new RootRecipientModificationParameters
            (
                MapEmailRecipients(dto.EmailRecipients),
                MapRoleRecipients(dto.RoleRecipients)
            ) : RootRecipientModificationParameters.Empty();
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