using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System.Linq;

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

        public Maybe<OperationError> UpdateNotificationUserRelations(int notificationId, IEnumerable<RecipientModel> updateModels)
        {
            using var transaction = _transactionManager.Begin();
            var notificationResult = _registrationNotificationService.GetNotificationById(notificationId);
            if (notificationResult.IsNone)
            {
                return new OperationError($"Notification with Id: {notificationId} was not found", OperationFailure.NotFound);
            }

            var notification = notificationResult.Value;
            if (!_authorizationContext.AllowModify(notification))
            {
                return new OperationError($"User is not allowed to modify notification with id: {notificationId}", OperationFailure.Forbidden);
            }

            var removeRelationsResult = DeleteUserRelationsByAdviceId(notificationId);
            if (removeRelationsResult.HasValue)
            {
                return removeRelationsResult.Value;
            }

            var newNotifications = updateModels.Select(updateModel => MapAdviceUserRelation(notificationId, updateModel)).ToList();
            _adviceUserRelationRepository.AddRange(newNotifications);
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
        
        private static AdviceUserRelation MapAdviceUserRelation(int notificationId, RecipientModel model)
        {
            return new AdviceUserRelation
            {
                AdviceId = notificationId,
                Email = model.Email,
                DataProcessingRegistrationRoleId = model.DataProcessingRegistrationRoleId,
                ItContractRoleId = model.ItContractRoleId,
                ItSystemRoleId = model.ItSystemRoleId,
                RecieverType = model.ReceiverType,
                RecpientType = model.RecipientType
            };
        }
    }
}
