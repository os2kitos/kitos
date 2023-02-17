using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class AdviceController : BaseEntityController<Advice>
    {
        private readonly IAdviceService _adviceService;
        private readonly IAdviceRootResolution _adviceRootResolution;
        private readonly IOperationClock _operationClock;
        private readonly ITransactionManager _transactionManager;
        private readonly IRegistrationNotificationService _registrationNotificationService;

        private readonly Regex _emailValidationRegex = new("([a-zA-Z\\-0-9\\._]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)");

        public AdviceController(
            IAdviceService adviceService,
            IGenericRepository<Advice> repository,
            IAdviceRootResolution adviceRootResolution,
            IOperationClock operationClock,
            ITransactionManager transactionManager,
            IRegistrationNotificationService registrationNotificationService)
            : base(repository)
        {
            _adviceService = adviceService;
            _adviceRootResolution = adviceRootResolution;
            _operationClock = operationClock;
            _transactionManager = transactionManager;
            _registrationNotificationService = registrationNotificationService;
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(_registrationNotificationService.GetCurrentUserNotifications());
        }

        [EnableQuery]
        public override IHttpActionResult Post(int organizationId, Advice advice)
        {
            if (advice.RelationId == null || advice.Type == null)
            {
                //Advice cannot be an orphan - it must belong to a root
                return BadRequest($"Both {nameof(advice.RelationId)} AND {nameof(advice.Type)} MUST be defined");
            }
            if (advice.Reciepients.Where(x => x.RecpientType == RecipientType.USER).Any(x => !_emailValidationRegex.IsMatch(x.Email)))
            {
                return BadRequest("Invalid email exists among receivers or CCs");
            }

            if (advice.AdviceType == AdviceType.Repeat)
            {
                if (advice.AlarmDate == null)
                {
                    return BadRequest("Start date is not set!");
                }

                if (advice.AlarmDate.Value.Date < _operationClock.Now.Date)
                {
                    return BadRequest("Start date is set before today");
                }

                if (advice.StopDate != null)
                {
                    if (advice.StopDate.Value.Date < advice.AlarmDate.Value.Date)
                    {
                        return BadRequest("Stop date is set before Start date");
                    }
                }

                if (advice.Scheduling is null)
                {
                    return BadRequest($"Scheduling must be defined when creating advice of type {nameof(AdviceType.Repeat)}");
                }
            }

            if (AllowCreate<Advice>(organizationId, advice) == false)
            {
                return Forbidden();
            }
            return _registrationNotificationService.Create(MapNotification(advice))
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

                if (!existingAdvice.IsActive)
                {
                    throw new ArgumentException(
                        "Cannot update inactive advice ");
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
                    .Update(key, MapBaseNotification<UpdateNotificationModel>(update))
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

        private static NotificationModel MapNotification(Advice notification)
        {
            var notificationModel = MapBaseNotification<NotificationModel>(notification);
            notificationModel.RepetitionFrequency = notification.Scheduling;
            notificationModel.FromDate = notification.AlarmDate;
            notificationModel.Recipients = MapRecipients(notification.Reciepients);

            return notificationModel;
        }

        private static TResult MapBaseNotification<TResult>(Advice notification) where TResult : UpdateNotificationModel, new()
        {
            return new TResult
            {
                Name = notification.Name,
                Subject = notification.Subject,
                Body = notification.Body,
                RelationId = notification.RelationId.GetValueOrDefault(),
                ToDate = notification.StopDate,
                Type = notification.Type,
                AdviceType = notification.AdviceType,
            };
        }

        private static IEnumerable<RecipientModel> MapRecipients(IEnumerable<AdviceUserRelation> recipients)
        {
            return recipients.Select(x => new RecipientModel
            {
                Email = x.Email,
                DataProcessingRegistrationRoleId = x.DataProcessingRegistrationRoleId,
                ItContractRoleId = x.ItContractRoleId,
                ItSystemRoleId = x.ItSystemRoleId,
                ReceiverType = x.RecieverType,
                RecipientType = x.RecpientType
            });
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