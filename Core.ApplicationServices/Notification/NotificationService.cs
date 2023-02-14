using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
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
                .Match(id => _registrationNotificationService.GetNotificationById(id),
                    () => new OperationError($"Id for notification with uuid: {uuid} was not found", OperationFailure.NotFound));
        }

        public Result<Advice, OperationError> CreateNotification(Guid organizationUuid, Advice notification)
        {
            throw new NotImplementedException();
        }

        public Result<Advice, OperationError> UpdateNotification(Guid organizationUuid, Advice notification)
        {
            throw new NotImplementedException();
        }
    }
}
