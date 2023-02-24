using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Time;
using Microsoft.Ajax.Utilities;
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

            if (AllowCreate<Advice>(organizationId, advice) == false)
            {
                return Forbidden();
            }

            if (advice.AdviceType == AdviceType.Immediate)
            {
                return MapCreateNotification<ImmediateNotificationModel>(advice)
                    .Bind(model => _registrationNotificationService.CreateImmediateNotification(model))
                    .Match(Created, FromOperationError);
            }

            return MapCreateNotification<ScheduledNotificationModel>(advice)
                .Bind(model =>
                {
                    MapBaseScheduledModel(model, advice);

                    model.FromDate = advice.AlarmDate.GetValueOrDefault();
                    model.RepetitionFrequency = advice.Scheduling.GetValueOrDefault();
                    return _registrationNotificationService.CreateScheduledNotification(model);
                })
                .Match(Created, FromOperationError);
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
                if (existingAdvice.AdviceType == AdviceType.Repeat)
                {
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
                }

                var entity = Repository.GetByKey(key);
                if (entity == null)
                {
                    return NotFound();
                }

                var validationError = ValidatePatch(delta, entity);
                if (validationError.HasValue)
                {
                    return BadRequest();
                }
                
                // calculate update
                var update = delta.Patch(entity);
                return _registrationNotificationService
                    .Update(key, MapUpdateModel(update))
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

        private static Result<TResult, OperationError> MapCreateNotification<TResult>(Advice notification) where TResult : class, IHasBaseNotificationPropertiesModel, IHasRecipientModels, new()
        {
            var notificationModel = new TResult
            {
                BaseProperties = MapBaseNotification(notification)
            };

            return MapRecipients(notification.Reciepients, RecieverType.CC, notificationModel.BaseProperties.Type)
                .Bind(ccs =>
                {
                    notificationModel.Ccs = ccs;
                    return MapRecipients(notification.Reciepients, RecieverType.RECIEVER,
                        notificationModel.BaseProperties.Type);
                })
                .Select(receivers =>
                {
                    notificationModel.Receivers = receivers;
                    return notificationModel;
                });
        }

        private static UpdateScheduledNotificationModel MapUpdateModel(Advice notification)
        {
            var model = new UpdateScheduledNotificationModel
            {
                BaseProperties = MapBaseNotification(notification)
            };
            MapBaseScheduledModel(model, notification);

            return model;
        }

        private static void MapBaseScheduledModel<T>(T model, Advice notification) where T : class, IHasName, IHasToDate
        {
            model.Name = notification.Name;
            model.ToDate = notification.StopDate;
        }

        private static BaseNotificationPropertiesModel MapBaseNotification(Advice notification)
        {
            return new BaseNotificationPropertiesModel
            {
                Subject = notification.Subject,
                Body = notification.Body,
                RelationId = notification.RelationId.GetValueOrDefault(),
                Type = notification.Type,
                AdviceType = notification.AdviceType,
            };
        }

        private static Result<RecipientModel, OperationError> MapRecipients(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType, RelatedEntityType type)
        {
            var recipientList = recipients.ToList();
            var recipient = new RecipientModel
            {
                EmailRecipients = recipientList.Where(x => x.RecpientType == RecipientType.USER && x.RecieverType == receiverType).Select(x => new EmailRecipientModel{Email = x.Email})
            };

            var roleRecipientsResult = MapRoleModels(recipientList, receiverType, type);
            if (roleRecipientsResult.Failed)
                return roleRecipientsResult.Error;

            recipient.RoleRecipients = roleRecipientsResult.Value;
            return recipient;
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

                result.Add(new RoleRecipientModel{RoleId = idResult.Value});
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