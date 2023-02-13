using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using System.Linq.Expressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Notification.Write;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.DomainModel.Advice;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationWriteModelMapper : WriteModelMapperBase, INotificationWriteModelMapper
    {
        public NotificationWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }
        public ImmediateNotificationModificationParameters FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto)
        {
            return MapImmediateNotificationWriteRequestDTO<ImmediateNotificationWriteRequestDTO, ImmediateNotificationModificationParameters>(dto, false);
        }

        public ScheduledNotificationWriteRequestDTO FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto)
        {
            throw new System.NotImplementedException();
        }

        public ScheduledNotificationWriteRequestDTO FromScheduledPut(UpdateScheduledNotificationWriteRequestDTO dto)
        {
            throw new System.NotImplementedException();
        }

        private TResult MapImmediateNotificationWriteRequestDTO<TDto, TResult>(TDto dto, bool enforceFallbackIfNotProvided)
            where TDto : ImmediateNotificationWriteRequestDTO
            where TResult : ImmediateNotificationModificationParameters
        {
            var rule = CreateChangeRule<TDto>(enforceFallbackIfNotProvided);
            TSection WithResetDataIfSectionIsNotDefined<TSection>(TSection deserializedValue,
                Expression<Func<TDto, TSection>> propertySelection) where TSection : new() =>
                WithResetDataIfPropertyIsDefined(deserializedValue,
                    propertySelection, enforceFallbackIfNotProvided);

            TSection WithResetDataIfSectionIsNotDefinedWithFallback<TSection>(TSection deserializedValue,
                Expression<Func<TDto, TSection>> propertySelection,
                Func<TSection> fallbackFactory) =>
                WithResetDataIfPropertyIsDefined(deserializedValue,
                    propertySelection, fallbackFactory, enforceFallbackIfNotProvided);

            dto.Ccs = WithResetDataIfSectionIsNotDefined(dto.Ccs, x => x.Ccs);
            dto.Receivers = WithResetDataIfSectionIsNotDefined(dto.Receivers, x => x.Receivers);

            return (TResult) new ImmediateNotificationModificationParameters
            {
                Body = rule.MustUpdate(x => x.Body) ? dto.Body.AsChangedValue() : OptionalValueChange<string>.None,
                Subject = rule.MustUpdate(x => x.Subject) ? dto.Body.AsChangedValue() : OptionalValueChange<string>.None,
                OwnerResourceUuid = rule.MustUpdate(x => x.OwnerResource) ? dto.OwnerResource?.Uuid.AsChangedValue() : OptionalValueChange<Guid>.None,
                Ccs = dto.Ccs.FromNullable().Select(x => MapRecipients(dto.Ccs, rule, true)),
                Receivers = dto.Receivers.FromNullable().Select(x => MapRecipients(dto.Ccs, rule, false))
            };
        }

        private ScheduledNotificationModificationParameters MapScheduledNotificationWriteRequestDTO<TDto>(TDto dto, IPropertyUpdateRule<TDto> rule,  bool enforceFallbackIfNotProvided)
            where TDto : ScheduledNotificationWriteRequestDTO
        {
            var parameters = MapImmediateNotificationWriteRequestDTO<TDto, ScheduledNotificationModificationParameters>(dto, enforceFallbackIfNotProvided);
            /*parameters.RepetitionFrequency = rule.MustUpdate(x => x.RepetitionFrequency)
                ? dto.RepetitionFrequency.AsChangedValue()
                : OptionalValueChange<Scheduling>.None;*/ //TODO: Create a mapper for the enum type
            parameters.FromDate = rule.MustUpdate(x => x.FromDate)
                ? dto.FromDate.AsChangedValue()
                : OptionalValueChange<DateTime>.None;

            return parameters;
        }

        private RecipientModificationParameters MapRecipients<TDto>(RecipientWriteRequestDTO dto, IPropertyUpdateRule<TDto> rule, bool isCc) where TDto : ImmediateNotificationWriteRequestDTO
        {
            return new RecipientModificationParameters
            {
                EmailRecipients = rule.MustUpdate(x => isCc ? x.Ccs.EmailRecipients : x.Receivers.EmailRecipients)
                    ? dto.EmailRecipients.FromNullable().Select(MapEmailRecipients).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<EmailRecipientModificationParameters>>>.None,
                RoleRecipients = rule.MustUpdate(x => isCc ? x.Ccs.RoleRecipients : x.Receivers.RoleRecipients)
                    ? dto.RoleRecipients.FromNullable().Select(MapRoleRecipients).AsChangedValue()
                    : OptionalValueChange<Maybe<IEnumerable<RoleRecipientModificationParameters>>>.None
            };
        }

        private IEnumerable<EmailRecipientModificationParameters> MapEmailRecipients(IEnumerable<EmailRecipientWriteRequestDTO> dto)
        {
            return dto.Select(x => new EmailRecipientModificationParameters
            {
                Email = x.Email
            }).ToList();
        }

        private IEnumerable<RoleRecipientModificationParameters> MapRoleRecipients(IEnumerable<RoleRecipientWriteRequestDTO> dto)
        {
            return dto.Select(x => new RoleRecipientModificationParameters
            {
                RoleUuid = x.RoleUuid
            }).ToList();
        }
    }
}