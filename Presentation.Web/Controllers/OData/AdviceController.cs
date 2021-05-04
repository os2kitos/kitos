using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Hangfire;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class AdviceController : BaseEntityController<Advice>
    {
        private readonly IAdviceService _adviceService;
        private readonly IGenericRepository<Advice> _repository;
        private readonly IGenericRepository<AdviceSent> _sentRepository;

        public AdviceController(
            IAdviceService adviceService,
            IGenericRepository<Advice> repository,
            IGenericRepository<AdviceSent> sentRepository)
            : base(repository)
        {
            _adviceService = adviceService;
            _repository = repository;
            _sentRepository = sentRepository;
        }

        [EnableQuery]
        public override IHttpActionResult Post(int organizationId, Advice advice)
        {
            var response = base.Post(organizationId, advice);

            if (response.GetType() == typeof(CreatedODataResult<Advice>))
            {
                var createdResponse = (CreatedODataResult<Advice>) response;
                var name = "Advice: " + createdResponse.Entity.Id;

                advice = createdResponse.Entity;
                advice.JobId = name;

                try
                {
                    _repository.Update(advice);
                    _repository.Save();
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to add advice", e);
                    return InternalServerError(e);
                }

                try
                {
                    if (advice.Scheduling == Scheduling.Immediate)
                    {
                        BackgroundJob.Enqueue(() => _adviceService.SendAdvice(createdResponse.Entity.Id));
                    }
                    else
                    {
                        RecurringJob.AddOrUpdate(name, () => _adviceService.SendAdvice(createdResponse.Entity.Id), CronStringHelper.CronPerInterval(advice.Scheduling.Value, advice.AlarmDate.Value));
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to schedule advice", e);
                    return InternalServerError(e);
                }
            }
            return response;
        }

        [EnableQuery]
        public override IHttpActionResult Patch(int key, Delta<Advice> delta)
        {
            var response = base.Patch(key, delta);

            if (response.GetType() == typeof(UpdatedODataResult<Advice>))
            {
                try
                {
                    var advice = delta.GetInstance();

                    RecurringJob.AddOrUpdate(advice.JobId, () => _adviceService.SendAdvice(key), CronStringHelper.CronPerInterval(advice.Scheduling.Value, advice.AlarmDate.Value));
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to update advice", e);
                    return InternalServerError(e);
                }
            }

            return response;
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

            if (anySents) return Forbidden();

            if (!AllowDelete(entity)) return Forbidden();

            try
            {
                RecurringJob.RemoveIfExists("Advice: " + key);
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            try
            {
                Repository.DeleteByKey(key);
                Repository.Save();
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to delete advice", e);
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}