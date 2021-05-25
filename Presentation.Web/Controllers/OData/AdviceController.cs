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
        private readonly IGenericRepository<AdviceSent> _sentRepository;
        private readonly IAdviceRootResolution _adviceRootResolution;

        private readonly Regex _emailValidationRegex = new Regex("([a-zA-Z\\-0-9\\.]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)");

        public AdviceController(
            IAdviceService adviceService,
            IGenericRepository<Advice> repository,
            IGenericRepository<AdviceSent> sentRepository,
            IAdviceRootResolution adviceRootResolution
            )
            : base(repository)
        {
            _adviceService = adviceService;
            _sentRepository = sentRepository;
            _adviceRootResolution = adviceRootResolution;
        }

        public override IHttpActionResult Get()
        {
            //TODO: Must not fallback to standard access control since that is not enough without global read access
            return base.Get();
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

                if (advice.AlarmDate.Value.Date < DateTime.Now.Date)
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
                    return InternalServerError(e);
                }
            }
            return response;
        }

        [EnableQuery]
        public override IHttpActionResult Patch(int key, Delta<Advice> delta)
        {
            try
            {
                var advice = delta.GetInstance();

                if (!advice.IsActive)
                {
                    throw new ArgumentException(
                        "Cannot update inactive advice ");
                }
                if (advice.AdviceType == AdviceType.Immediate)
                {
                    throw new ArgumentException("Editing is not allowed for immediate advice");
                }
                if (advice.AdviceType == AdviceType.Repeat)
                {
                    var changedPropertyNames = delta.GetChangedPropertyNames().ToList();
                    if (changedPropertyNames.All(IsRecurringEditableProperty))
                    {
                        throw new ArgumentException("For recurring advices editing is only allowed for name, subject and stop date");
                    }

                    if (changedPropertyNames.Contains("StopDate"))
                    {
                        if (advice.StopDate <= advice.AlarmDate || advice.StopDate <= DateTime.Now)
                        {
                            throw new ArgumentException("For recurring advices only future stop dates after the set alarm date is allowed");
                        }
                    }
                }

                var response = base.Patch(key, delta);

                if (response is UpdatedODataResult<Advice>)
                {
                    _adviceService.RescheduleRecurringJob(advice);
                }

                return response;
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to update advice", e);
                return InternalServerError(e);
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
            if (entity == null) return NotFound();

            var anySents = _sentRepository.AsQueryable().Any(m => m.AdviceId == key);

            if (anySents)
            {
                return BadRequest("Cannot delete advice which has been sent");
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
                return InternalServerError(e);
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
                return InternalServerError(e);
            }
            return Updated(entity);
        }
    }
}