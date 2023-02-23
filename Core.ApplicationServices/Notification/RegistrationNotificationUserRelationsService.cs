using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System.Linq;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Notification
{
    public class RegistrationNotificationUserRelationsService : IRegistrationNotificationUserRelationsService
    {
        private readonly IGenericRepository<AdviceUserRelation> _adviceUserRelationRepository;
        private readonly IRegistrationNotificationService _registrationNotificationService;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizationContext _authorizationContext;

        public RegistrationNotificationUserRelationsService(IGenericRepository<AdviceUserRelation> adviceUserRelationRepository, 
            IRegistrationNotificationService registrationNotificationService,
            ITransactionManager transactionManager, 
            IAuthorizationContext authorizationContext)
        {
            _adviceUserRelationRepository = adviceUserRelationRepository;
            _registrationNotificationService = registrationNotificationService;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
        }

        public Maybe<OperationError> UpdateNotificationUserRelations(int notificationId, RecipientModel ccRecipients, RecipientModel receiverRecipients, RelatedEntityType relatedEntityType)
        {
            using var transaction = _transactionManager.Begin();

            var error = _registrationNotificationService.GetNotificationById(notificationId)
                .Match
                (
                    notification => 
                        _authorizationContext.AllowModify(notification)
                            ? DeleteUserRelationsByAdviceId(notificationId)
                            : new OperationError($"User is not allowed to modify notification with id: {notificationId}", OperationFailure.Forbidden),
                    error => error);

            if (error.HasValue)
                return error;

            var recipients = new List<AdviceUserRelation>();
            recipients.AddRange(MapAdviceUserRelation(notificationId, ccRecipients, RecieverType.CC, relatedEntityType));
            recipients.AddRange(MapAdviceUserRelation(notificationId, receiverRecipients, RecieverType.RECIEVER, relatedEntityType));

            _adviceUserRelationRepository.AddRange(recipients);
            _adviceUserRelationRepository.Save();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> DeleteUserRelationsByAdviceId(int notificationId)
        {
            foreach (var d in _adviceUserRelationRepository.AsQueryable().Where(d => d.AdviceId == notificationId).ToList())
            {
                if (_authorizationContext.AllowDelete(d))
                {
                    _adviceUserRelationRepository.Delete(d);
                    _adviceUserRelationRepository.Save();
                }
                else
                {
                    return new OperationError($"User is not allowed to delete user relation with adviceId: {notificationId}", OperationFailure.Forbidden);
                }
            }
            return Maybe<OperationError>.None;
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
    }
}
