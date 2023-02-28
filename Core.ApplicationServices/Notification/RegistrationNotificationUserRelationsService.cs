using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System.Linq;
using Core.DomainModel.Shared;
using Core.DomainModel;
using Core.DomainServices.Advice;

namespace Core.ApplicationServices.Notification
{
    public class RegistrationNotificationUserRelationsService : IRegistrationNotificationUserRelationsService
    {
        private readonly IGenericRepository<AdviceUserRelation> _adviceUserRelationRepository;
        private readonly IRegistrationNotificationService _registrationNotificationService;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IAdviceRootResolution _adviceRootResolution;

        public RegistrationNotificationUserRelationsService(IGenericRepository<AdviceUserRelation> adviceUserRelationRepository, 
            IRegistrationNotificationService registrationNotificationService,
            ITransactionManager transactionManager, 
            IAuthorizationContext authorizationContext, 
            IAdviceRootResolution adviceRootResolution)
        {
            _adviceUserRelationRepository = adviceUserRelationRepository;
            _registrationNotificationService = registrationNotificationService;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
            _adviceRootResolution = adviceRootResolution;
        }

        public Result<Advice, OperationError> UpdateNotificationUserRelations(int notificationId, RecipientModel ccRecipients, RecipientModel receiverRecipients, RelatedEntityType relatedEntityType)
        {
            using var transaction = _transactionManager.Begin();

            var notificationResult = _registrationNotificationService.GetNotificationById(notificationId)
                .Bind(notification => 
                        _authorizationContext.AllowModify(ResolveRoot(notification))
                            ? DeleteUserRelationsByAdviceId(notification)
                            : new OperationError($"User is not allowed to modify notification with id: {notificationId}", OperationFailure.Forbidden)
                );
            if (notificationResult.Failed)
                return notificationResult.Error;
            var notification = notificationResult.Value;

            var recipients = new List<AdviceUserRelation>();
            recipients.AddRange(MapAdviceUserRelation(notificationId, ccRecipients, RecieverType.CC, relatedEntityType));
            recipients.AddRange(MapAdviceUserRelation(notificationId, receiverRecipients, RecieverType.RECIEVER, relatedEntityType));

            _adviceUserRelationRepository.AddRange(recipients);
            _adviceUserRelationRepository.Save();
            transaction.Commit();

            return notification;
        }

        private Result<Advice, OperationError> DeleteUserRelationsByAdviceId(Advice notification)
        {
            foreach (var d in _adviceUserRelationRepository.AsQueryable().Where(d => d.AdviceId == notification.Id).ToList())
            {
                if (_authorizationContext.AllowDelete(d))
                {
                    _adviceUserRelationRepository.Delete(d);
                    _adviceUserRelationRepository.Save();
                }
                else
                {
                    return new OperationError($"User is not allowed to delete user relation with adviceId: {notification.Id}", OperationFailure.Forbidden);
                }
            }
            return notification;
        }
        
        private static IEnumerable<AdviceUserRelation> MapAdviceUserRelation(int notificationId, RecipientModel model, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            var recipients = new List<AdviceUserRelation>();
            recipients.AddRange(model.RoleRecipients.Select(x => new AdviceUserRelation
            {
                AdviceId = notificationId,
                DataProcessingRegistrationRoleId = relatedEntityType == RelatedEntityType.dataProcessingRegistration ? x.RoleId : null,
                ItContractRoleId = relatedEntityType == RelatedEntityType.itContract? x.RoleId : null,
                ItSystemRoleId = relatedEntityType == RelatedEntityType.itSystemUsage ? x.RoleId : null,
                RecieverType = receiverType,
                RecpientType = RecipientType.ROLE
            }));
            recipients.AddRange(model.EmailRecipients.Select(x => new AdviceUserRelation
            {
                AdviceId = notificationId,
                Email = x.Email,
                RecieverType = receiverType,
                RecpientType = RecipientType.USER
            }));

            return recipients;
        }
        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }
    }
}
