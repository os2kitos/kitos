using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Time;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class AdviceController : BaseEntityController<Advice>
    {
        private readonly IAdviceRootResolution _adviceRootResolution;
        private readonly IOperationClock _operationClock;
        private readonly IRegistrationNotificationService _registrationNotificationService;

        public AdviceController(
            IGenericRepository<Advice> repository,
            IAdviceRootResolution adviceRootResolution,
            IOperationClock operationClock,
            IRegistrationNotificationService registrationNotificationService)
            : base(repository)
        {
            _adviceRootResolution = adviceRootResolution;
            _operationClock = operationClock;
            _registrationNotificationService = registrationNotificationService;
        }

        [NonAction]
        public override IHttpActionResult Get() => throw new NotImplementedException();

        [EnableQuery]
        public override IHttpActionResult Post(int organizationId, Advice advice)
        {
            if (advice.RelationId == null || advice.Type == null)
            {
                //Advice cannot be an orphan - it must belong to a root
                return BadRequest($"Both {nameof(advice.RelationId)} AND {nameof(advice.Type)} MUST be defined");
            }

            if (advice.AdviceType == AdviceType.Repeat)
            {
                if (advice.AlarmDate == null)
                {
                    return BadRequest("Start date is not set!");
                }
            }

            var recipientsResult = MapBothRecipientTypes(advice);
            if (recipientsResult.Failed)
                return FromOperationError(recipientsResult.Error);
            var recipients = recipientsResult.Value;

            if (advice.AdviceType == AdviceType.Immediate)
            {
                var immediateNotificationModel = new ImmediateNotificationModel(MapBaseNotificationProperties(advice), recipients.ccs, recipients.receivers);
                return _registrationNotificationService.CreateImmediateNotification(immediateNotificationModel)
                    .Match(Created, FromOperationError);
            }


            if (advice.AlarmDate == null)
            {
                return BadRequest("Start date is not set!");

            }
            if (advice.Scheduling is null or Scheduling.Immediate)
            {
                return BadRequest($"Scheduling must be defined and cannot be {nameof(Scheduling.Immediate)} when creating advice of type {nameof(AdviceType.Repeat)}");
            }

            var scheduledNotificationModel = new ScheduledNotificationModel(advice.Name, advice.StopDate,
                advice.Scheduling, advice.AlarmDate,
                MapBaseNotificationProperties(advice), recipients.ccs, recipients.receivers);
            return _registrationNotificationService.CreateScheduledNotification(scheduledNotificationModel).Match(Created, FromOperationError);
        }

        [EnableQuery]
        public override IHttpActionResult Patch(int key, Delta<Advice> delta)
        {
            try
            {
                var existingAdvice = Repository.GetByKey(key);
                var deltaAdvice = delta.GetInstance();

                if (existingAdvice == null)
                {
                    return NotFound();
                }

                if (existingAdvice.Type != deltaAdvice.Type)
                {
                    return BadRequest("Cannot change advice type");
                }
                if (existingAdvice.AdviceType == AdviceType.Immediate)
                {
                    throw new ArgumentException("Editing is not allowed for immediate advice");
                }

                var changedPropertyNames = delta.GetChangedPropertyNames().ToList();
                if (changedPropertyNames.All(IsRecurringEditableProperty))
                {
                    throw new ArgumentException("For recurring advices editing is only allowed for name, subject and stop date");
                }

                if (changedPropertyNames.Contains("StopDate") && deltaAdvice.StopDate != null)
                {
                    if (deltaAdvice.StopDate.Value.Date < deltaAdvice.AlarmDate.GetValueOrDefault().Date || deltaAdvice.StopDate.Value.Date < _operationClock.Now.Date)
                    {
                        throw new ArgumentException("For recurring advices only future stop dates after the set alarm date is allowed");
                    }
                }

                var entity = Repository.GetByKey(key);
                if (entity == null)
                {
                    return NotFound();
                }

                var validationError = ValidatePatch(delta, entity);
                if (validationError.HasValue)
                {
                    return validationError.Value;
                }
                
                // calculate update
                var update = delta.Patch(entity);

                if (update.RelationId == null)
                {
                    //Advice cannot be an orphan - it must belong to a root
                    return BadRequest($"{nameof(update.RelationId)} MUST be defined");
                }

                return _registrationNotificationService
                    .Update(key, new UpdateScheduledNotificationModel(update.Name, update.AlarmDate, MapBaseNotificationProperties(update)))
                    .Match(Ok, FromOperationError);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to update advice", e);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }


        [EnableQuery]
        [ODataRoute("GetAdvicesByOrganizationId(organizationId={organizationId})")]
        public IHttpActionResult GetAdvicesByOrganizationId([FromODataUri] int organizationId)
        {
            return _registrationNotificationService.GetNotificationsByOrganizationId(organizationId)
                .Match(Ok, FromOperationError);
        }

        [EnableQuery]
        public override IHttpActionResult Delete(int key)
        {
            return _registrationNotificationService.Delete(key)
                .Match(FromOperationError, () => StatusCode(HttpStatusCode.NoContent));
        }

        [HttpPatch]
        [EnableQuery]
        [ODataRoute("DeactivateAdvice")]
        public IHttpActionResult DeactivateAdvice([FromODataUri] int key)
        {
            return _registrationNotificationService.DeactivateNotification(key)
                .Match(Updated, FromOperationError);
        }


        protected override void RaiseCreatedDomainEvent(Advice entity)
        {
            RaiseAsRootModification(entity);
        }

        protected override void RaiseDeletedDomainEvent(Advice entity)
        {
            RaiseAsRootModification(entity);
        }

        protected override void RaiseUpdatedDomainEvent(Advice entity)
        {
            RaiseAsRootModification(entity);
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Advice, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
        }

        private static Result<(RecipientModel ccs, RecipientModel receivers), OperationError> MapBothRecipientTypes(Advice notification)
        {

            return MapRecipients(notification.Reciepients, RecieverType.CC, notification.Type)
                .Bind(ccs =>
                {
                    return MapRecipients(notification.Reciepients, RecieverType.RECIEVER, notification.Type)
                        .Select(receivers => (ccs, receivers));
                });
        }

        private static BaseNotificationPropertiesModel MapBaseNotificationProperties(Advice notification)
        {
            return new BaseNotificationPropertiesModel
            (
                notification.Subject,
                notification.Body,
                notification.Type,
                notification.AdviceType,
                notification.RelationId.Value
            );
        }

        private static Result<RecipientModel, OperationError> MapRecipients(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType, RelatedEntityType type)
        {
            var recipientList = recipients.ToList();

            var roleRecipientsResult = MapRoleModels(recipientList, receiverType, type);
            if (roleRecipientsResult.Failed)
                return roleRecipientsResult.Error;

            var emailRecipients = recipientList
                .Where(x => x.RecpientType == RecipientType.USER && x.RecieverType == receiverType)
                .Select(x => new EmailRecipientModel(x.Email));
            
            return new RecipientModel(emailRecipients, roleRecipientsResult.Value);
        }

        private static Result<IEnumerable<RoleRecipientModel>, OperationError> MapRoleModels(IEnumerable<AdviceUserRelation> recipients,
            RecieverType receiverType, RelatedEntityType type)
        {
            var result = new List<RoleRecipientModel>();
            foreach (var adviceUserRelation in recipients.Where(x => x.RecpientType == RecipientType.ROLE && x.RecieverType == receiverType))
            {
                var idResult = ResolveRoleId(adviceUserRelation, type);
                if (idResult.IsNone)
                    return new OperationError("Role id cannot be null", OperationFailure.BadInput);

                result.Add(new RoleRecipientModel(idResult.Value));
            }

            return result;
        }

        private static Maybe<int> ResolveRoleId(AdviceUserRelation recipient, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    return recipient.DataProcessingRegistrationRoleId;
                case RelatedEntityType.itSystemUsage:
                    return recipient.ItSystemRoleId;
                case RelatedEntityType.itContract:
                    return recipient.ItContractRoleId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }
        
        private void RaiseAsRootModification(Advice entity)
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

        private static bool IsRecurringEditableProperty(string name)
        {
            return name.Equals("Name") || name.Equals("Subject") || name.Equals("StopDate");
        }
    }
}