using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Configuration.Annotations;
using Core.DomainModel.Advice;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationResponseMapper : INotificationResponseMapper
    {
        public NotificationResponseDTO MapNotificationResponseDTO(Advice notification)
        {
            return new NotificationResponseDTO
            {
                Active = notification.IsActive,
                Name = notification.Name,
                LastSent = notification.SentDate,
                FromDate = notification.AlarmDate,
                ToDate = notification.StopDate,
                Subject = notification.Subject,
                Receivers = MapRecipients(notification, RecieverType.RECIEVER),
                CCs = MapRecipients(notification, RecieverType.CC),
                OwnerResource = notification.ObjectOwner.MapIdentityNamePairDTO(),
                Uuid = notification.Uuid
            };
        }

        private RecipientResponseDTO MapRecipients(Advice notification, RecieverType receiverType)
        {
            var recipientsByType = notification.Reciepients.Where(x => x.RecieverType == receiverType).ToList();
            return MapRecipientResponseDTO(recipientsByType);
        }

        private RecipientResponseDTO MapRecipientResponseDTO(IEnumerable<AdviceUserRelation> recipients)
        {
            var recipientList = recipients.ToList();
            var recipient = new RecipientResponseDTO
            {
                EmailRecipients = MapEmailRecipientResponseDTOs(recipientList),
                RoleRecipients = MapRoleRecipientResponseDTOs(recipientList)
            };
            return recipient;
        }

        private IEnumerable<EmailRecipientResponseDTO> MapEmailRecipientResponseDTOs(IEnumerable<AdviceUserRelation> recipients)
        {
            return recipients.Select(MapEmailRecipientResponseDTO);
        }

        private EmailRecipientResponseDTO MapEmailRecipientResponseDTO(AdviceUserRelation notification)
        {
            return new EmailRecipientResponseDTO
            {
                Email = notification.Email
            };
        }

        private IEnumerable<RoleRecipientResponseDTO> MapRoleRecipientResponseDTOs(IEnumerable<AdviceUserRelation> recipients)
        {
            return recipients.Select(MapRoleRecipientResponseDTO);
        }

        private RoleRecipientResponseDTO MapRoleRecipientResponseDTO(AdviceUserRelation notification)
        {
            return new RoleRecipientResponseDTO
            {
                Role = new IdentityNamePairResponseDTO(new Guid(), "")//notification.GetRole();
            };
        }
    }
}