using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Authorization;
using Core.DomainServices.Time;
using Infrastructure.Services.DomainEvents;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class AdviceController : BaseEntityController<Advice>
    {
        private readonly IAdviceService _adviceService;
        private readonly IAdviceRootResolution _adviceRootResolution;
        private readonly IOperationClock _operationClock;

        private readonly Regex _emailValidationRegex = new Regex("([a-zA-Z\\-0-9\\.]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)");

        public AdviceController(
            IAdviceService adviceService,
            IGenericRepository<Advice> repository,
            IAdviceRootResolution adviceRootResolution,
            IOperationClock operationClock
            )
            : base(repository)
        {
            _adviceService = adviceService;
            _adviceRootResolution = adviceRootResolution;
            _operationClock = operationClock;
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(_adviceService.GetAdvicesAccessibleToCurrentUser());
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<Advice, IEntityWithAdvices>(ResolveRoot, base.GetCrudAuthorization());
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
                case ItProject root:
                    DomainEvents.Raise(new EntityUpdatedEvent<ItProject>(root));
                    break;
                case DataProcessingRegistration root:
                    DomainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(root));
                    break;
            }
        }

        [EnableQuery]
        public override IHttpActionResult Post(int organizationId, Advice advice)
        {
            if (advice.RelationId == null || advice.Type == null)
            {
                //Advice cannot be an orphan - it must belong to a root
                return BadRequest($"Both {nameof(advice.RelationId)} AND {nameof(advice.Type)} MUST be defined");
            }
            if (advice.Reciepients.Where(x => x.RecpientType == RecieverType.USER).Any(x => !_emailValidationRegex.IsMatch(x.Name)))
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

                if (advice.Scheduling == null || advice.Scheduling == Scheduling.Immediate)
                {
                    return BadRequest($"Scheduling must be defined and cannot be {nameof(Scheduling.Immediate)} when creating advice of type {nameof(AdviceType.Repeat)}");
                }
            }

            //Prepare new advice
            advice.IsActive = true;
            if (advice.AdviceType == AdviceType.Immediate)
            {
                advice.Scheduling = Scheduling.Immediate;
                advice.StopDate = null;
                advice.AlarmDate = null;
            }

            var response = base.Post(organizationId, advice);

            if (response.GetType() == typeof(CreatedODataResult<Advice>))
            {
                var createdResponse = (CreatedODataResult<Advice>)response;
                var name = "Advice: " + createdResponse.Entity.Id;

                advice = createdResponse.Entity;
                advice.JobId = name;
                advice.IsActive = true;

                try
                {
                    _adviceService.CreateAdvice(advice);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to add advice", e);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            return response;
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

                var response = base.Patch(key, delta);

                if (response is UpdatedODataResult<Advice>)
                {
                    var updatedAdvice = Repository.GetByKey(key); //Re-load
                    _adviceService.RescheduleRecurringJob(updatedAdvice);
                }

                return response;
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to update advice", e);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        private static bool IsRecurringEditableProperty(string name)
        {
            return name.Equals("Name") || name.Equals("Subject") || name.Equals("StopDate");
        }

        [EnableQuery]
        [ODataRoute("GetAdvicesByOrganizationId(organizationId={organizationId})")]
        public IHttpActionResult GetAdvicesByOrganizationId([FromODataUri] int organizationId)
        {
            return GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All
                ? Forbidden()
                : Ok(_adviceService.GetAdvicesForOrg(organizationId));
        }

        [EnableQuery]
        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == key);
            if (entity == null)
            {
                return NotFound();
            }
            if (!entity.CanBeDeleted)
            {
                return BadRequest("Cannot delete advice which is active or has been sent");
            }
            if (!AllowDelete(entity))
            {
                return Forbidden();
            }

            try
            {
                _adviceService.Delete(entity);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to delete advice", e);
                return StatusCode(HttpStatusCode.InternalServerError);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]
        [EnableQuery]
        [ODataRoute("DeactivateAdvice")]
        public IHttpActionResult DeactivateAdvice([FromODataUri] int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == key);
            if (entity == null) return NotFound();

            try
            {
                if (!AllowModify(entity))
                {
                    return Forbidden();
                }
                _adviceService.Deactivate(entity);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to delete advice", e);
                return StatusCode(HttpStatusCode.InternalServerError);
            }
            return Updated(entity);
        }
    }
}