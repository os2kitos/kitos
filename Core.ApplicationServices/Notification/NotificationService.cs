using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IRegistrationNotificationService _registrationNotificationService;
        private readonly IAuthorizationContext _authorizationContext;

        public NotificationService(IEntityIdentityResolver entityIdentityResolver,
            IRegistrationNotificationService registrationNotificationService, 
            IAuthorizationContext authorizationContext)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _registrationNotificationService = registrationNotificationService;
            _authorizationContext = authorizationContext;
        }

        public Result<IEnumerable<Advice>, OperationError> GetAdvices(Guid organizationUuid, RelatedEntityType relatedEntityType, DateTime fromDate)
        {
            throw new NotImplementedException();
        }

        public Result<Advice, OperationError> GetAdviceByUuid(Guid uuid, RelatedEntityType relatedEntityType)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(uuid)
                .Match(id => _registrationNotificationService.GetNotificationById(id)
                    .Match<Result<Advice, OperationError>>
                    (
                        notification => notification, 
                        () => new OperationError($"Notification with Id: {id} was not found", OperationFailure.NotFound)
                    ),
                () => new OperationError($"Id for notification with uuid: {uuid} was not found", OperationFailure.NotFound));
        }

        public Result<Advice, OperationError> CreateImmediateNotification(Guid organizationUuid, ImmediateNotificationModificationParameters parameters)
        {
            var relationIdResult = ResolveRelationId(parameters.OwnerResourceUuid, parameters.Type);
            if (relationIdResult.IsNone)
                return new OperationError($"Id for owner type with uuid: {parameters.OwnerResourceUuid} was not found" , OperationFailure.NotFound);
            var relationId = relationIdResult.Value;
            var model = new NotificationModel()
            {
                AdviceType = AdviceType.Immediate,
                Body = parameters.Body,
                Type = parameters.Type,
                RelationId = relationId
                //TODO: map recipients
            };
            throw new NotImplementedException();
        }

        public Result<Advice, OperationError> CreateScheduledNotification(Guid organizationUuid, ScheduledNotificationModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public Result<Advice, OperationError> UpdateScheduledNotification(Guid organizationUuid, UpdateScheduledNotificationModificationParameters parameters)
        {
            throw new NotImplementedException();
        }

        private Maybe<int> ResolveRelationId(Guid relationUuid, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(relationUuid),
                RelatedEntityType.itContract => _entityIdentityResolver.ResolveDbId<ItContract>(relationUuid),
                RelatedEntityType.itSystemUsage => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(relationUuid),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }
    }
}
