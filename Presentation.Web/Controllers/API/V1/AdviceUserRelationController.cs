using Core.DomainModel.Advice;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Advice;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models.API.V1;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.Model.Notification;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Shared;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    public class AdviceUserRelationController : GenericApiController<AdviceUserRelation, AdviceUserRelationDTO>
    {
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IAdviceRootResolution _adviceRootResolution;
        private readonly IRegistrationNotificationUserRelationsService _registrationNotificationUserRelationsService;

        public AdviceUserRelationController(
            IGenericRepository<AdviceUserRelation> repository,
            IGenericRepository<Advice> adviceRepository,
            IAdviceRootResolution adviceRootResolution,
            IRegistrationNotificationUserRelationsService registrationNotificationUserRelationsService)
            : base(repository)
        {
            _adviceRepository = adviceRepository;
            _adviceRootResolution = adviceRootResolution;
            _registrationNotificationUserRelationsService = registrationNotificationUserRelationsService;
        }

        [NonAction]
        public override HttpResponseMessage GetAll(PagingModel<AdviceUserRelation> paging) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage GetSingle(int id) => throw new NotSupportedException();

        [NonAction]
        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj) => throw new NotSupportedException();

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<AdviceUserRelation, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }

        private IEntityWithAdvices ResolveRoot(AdviceUserRelation relation)
        {
            if (relation?.AdviceId.HasValue == true)
            {
                var advice = _adviceRepository.GetByKey(relation.AdviceId.Value);
                if (advice != null)
                {
                    return ResolveRoot(advice);
                }
            }

            return null;
        }

        protected override void RaiseDeleted(AdviceUserRelation entity)
        {
            RaiseAsRootModification(entity);
        }

        protected override void RaiseNewObjectCreated(AdviceUserRelation savedItem)
        {
            RaiseAsRootModification(savedItem);
        }

        protected override void RaiseUpdated(AdviceUserRelation item)
        {
            RaiseAsRootModification(item);
        }

        private void RaiseAsRootModification(AdviceUserRelation entity)
        {
            switch (ResolveRoot(entity))
            {
                case ItContract root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItContract>(root));
                    break;
                case ItSystemUsage root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(root));
                    break;
                case DataProcessingRegistration root:
                    DomainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(root));
                    break;
            }
        }

        /// <summary>
        /// Sletter adviser med det specificerede id fra en genereisk advis
        /// </summary>
        /// <param name="adviceId"></param>
        /// <returns></returns>
        [NonAction]
        public virtual HttpResponseMessage DeleteByAdviceId(int adviceId) => throw new NotSupportedException();
        /// <summary>
        /// Update range
        /// </summary>
        /// <param name="adviceId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/AdviceUserRelation/{{notificationId}}/{relatedEntityType}/update-range")]
        public HttpResponseMessage UpdateRange(int notificationId, RelatedEntityType relatedEntityType, [FromBody] IEnumerable<AdviceUserRelation> body)
        {
            var recipients = body.ToList();
            return MapRecipients(recipients, RecieverType.CC)
                .Bind(ccs => MapRecipients(recipients, RecieverType.RECIEVER)
                    .Select(receivers => (ccs, receivers)))
                .Bind(result => _registrationNotificationUserRelationsService.UpdateNotificationUserRelations(notificationId, result.ccs, result.receivers, relatedEntityType))
                .Match(Ok, FromOperationError);
        }

        private static Result<RecipientModel, OperationError> MapRecipients(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType)
        {
            var recipientList = recipients.ToList();
            var roleRecipientsResult = MapRoleModels(recipientList, receiverType);
            if (roleRecipientsResult.Failed)
                return roleRecipientsResult.Error;

            var emailRecipients = recipientList
                .Where(x => x.RecpientType == RecipientType.USER && x.RecieverType == receiverType)
                .Select(x => new EmailRecipientModel (x.Email))
                .ToList();

            var recipient = new RecipientModel(emailRecipients, roleRecipientsResult.Value);
            
            return recipient;
        }

        private static Result<IEnumerable<RoleRecipientModel>, OperationError> MapRoleModels(IEnumerable<AdviceUserRelation> recipients,
            RecieverType receiverType)
        {
            var result = new List<RoleRecipientModel>();
            foreach (var adviceUserRelation in recipients.Where(x => x.RecpientType == RecipientType.ROLE && x.RecieverType == receiverType))
            {
                var idResult = ResolveRoleId(adviceUserRelation);
                if (idResult.IsNone)
                    return new OperationError("Role id cannot be null", OperationFailure.BadInput);

                result.Add(new RoleRecipientModel(idResult.Value));
            }

            return result;
        }

        private static Maybe<int> ResolveRoleId(AdviceUserRelation recipient)
        {
            return recipient.DataProcessingRegistrationRoleId ?? recipient.ItContractRoleId ?? recipient.ItSystemRoleId;
        }
    }
}