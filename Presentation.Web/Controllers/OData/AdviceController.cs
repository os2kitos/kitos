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
            if (advice.AdviceType == AdviceType.Immediate)
            {

            }
            else if (advice.AdviceType == AdviceType.Repeat)
            {
                if (advice.AlarmDate.Value.Date < DateTime.Now.Date)
                {
                    return BadRequest("Start date is set before today");
                }
            }


            var response = base.Post(organizationId, advice);

            if (response.GetType() == typeof(CreatedODataResult<Advice>))
            {
                var createdResponse = (CreatedODataResult<Advice>) response;
                var name = "Advice: " + createdResponse.Entity.Id;

                advice = createdResponse.Entity;
                advice.JobId = name;
                advice.IsActive = true;

                try
                {
                    UpdateRepository(advice);
                    ScheduleAdvice(advice, createdResponse, name);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to add advice", e);
                    return InternalServerError(e);
                }
            }
            return response;
        }

        private void ScheduleAdvice(Advice advice, CreatedODataResult<Advice> createdResponse, string name)
        {
            if (advice.AdviceType == AdviceType.Immediate)
            {
                BackgroundJob.Enqueue(() => _adviceService.SendAdvice(createdResponse.Entity.Id));
            }
            else if(advice.AdviceType == AdviceType.Repeat)
            {

                BackgroundJob.Schedule(
                    () => CreateDelayedRecurringJob(createdResponse.Entity.Id, name, advice.Scheduling.Value,
                        advice.AlarmDate.Value), new DateTimeOffset(advice.AlarmDate.Value));
            }
        }

        private void UpdateRepository(Advice advice)
        {
            _repository.Update(advice);
            _repository.Save();
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
                    BackgroundJob.Schedule(() => CreateDelayedRecurringJob(key, advice.JobId, advice.Scheduling.Value, advice.AlarmDate.Value), new DateTimeOffset(advice.AlarmDate.Value));
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to update advice", e);
                    return InternalServerError(e);
                }
            }

            return response;
        }

        public void CreateDelayedRecurringJob(int entityId, string name, Scheduling schedule, DateTime alarmDate)
        {
            RecurringJob.AddOrUpdate(name, () => _adviceService.SendAdvice(entityId), CronStringHelper.CronPerInterval(schedule, alarmDate));
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
                DeleteJobFromHangfire(key, entity);
                DeleteFromRepository(key);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to delete advice", e);
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        private void DeleteFromRepository(int key)
        {
            Repository.DeleteByKey(key);
            Repository.Save();
        }

        private static void DeleteJobFromHangfire(int key, Advice entity)
        {
            DeletePostponedRecurringJob(entity.JobId);
            RecurringJob.RemoveIfExists("Advice: " + key);
        }

        private static void DeletePostponedRecurringJob(string textId)
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var jobsScheduled = monitor.ScheduledJobs(0, int.MaxValue).
                Where(x => x.Value.Job.Method.Name == "CreateDelayedRecurringJob");
            foreach (var j in jobsScheduled)
            {
                var t = j.Value.Job.Args[1].ToString(); // Pick "Advice: nn"
                if (t.Contains(textId))
                {
                    BackgroundJob.Delete(j.Key);
                }
            }
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
                DeleteJobFromHangfire(key, entity);
                entity.IsActive = false;
                UpdateRepository(entity);
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