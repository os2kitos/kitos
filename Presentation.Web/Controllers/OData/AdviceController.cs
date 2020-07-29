using Core.ApplicationServices;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Hangfire;
using System;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Results;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    using Core.DomainModel.AdviceSent;
    using System.Net;

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
        public override IHttpActionResult Post(Advice advice)
        {
            var response = base.Post(advice);

            if (response.GetType() == typeof(CreatedODataResult<Advice>)) {
                
                var createdRepsonse = (CreatedODataResult<Advice>)response ;
                var name = "Advice: " + createdRepsonse.Entity.Id;

                advice = createdRepsonse.Entity;
                advice.JobId = name;

                try
                {
                    _repository.Update(advice);
                    _repository.Save();
                }
                catch (Exception e) {
                    Logger.ErrorException("Failed to add advice",e);
                    return InternalServerError(e);
                }

                try
                {
                   switch (advice.Scheduling) {
                            case Scheduling.Immediate:
                            var jobId = BackgroundJob.Enqueue(
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id));
                                break;
                            case Scheduling.Hour:
                            
                            string cron = "0 * * * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Day:

                             var month = advice.AlarmDate.Value.Month;
                             var day = advice.AlarmDate.Value.Day;
                             cron = "0 8 * * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Week:
                            string weekDay = advice.AlarmDate.Value.DayOfWeek.ToString().Substring(0, 3);
                            cron = "0 8 *  * " + weekDay;

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Month:

                            day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Year:

                             month = advice.AlarmDate.Value.Month;
                             day = advice.AlarmDate.Value.Day;
                             cron = "0 8 " + day + " " + month + " *";
                                
                                RecurringJob.AddOrUpdate(name,
                    () => _adviceService.SendAdvice(createdRepsonse.Entity.Id),
                    cron);
                            break;
                        }
                }
                catch (Exception e) {
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

                    switch (advice.Scheduling)
                    {
                        case Scheduling.Hour:

                            string cron = "0 * * * *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.SendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Day:
                            
                            cron = "0 8 * * *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.SendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Week:
                            string weekDay = advice.AlarmDate.Value.DayOfWeek.ToString().Substring(0, 3);
                            cron = "0 8 *  * " + weekDay;

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.SendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Month:

                            var day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " * *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.SendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Year:

                            var month = advice.AlarmDate.Value.Month;
                            day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " " + month + " *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                () => _adviceService.SendAdvice(key),
                cron);
                            break;
                    }
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
        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == key);
            if (entity == null)
            {
                return NotFound();
            }

            var anySents = _sentRepository.AsQueryable().Any(m => m.AdviceId == key);

            if (anySents) {
                return Forbidden();
            }

            if (!AllowDelete(entity))
            {
                return Forbidden();
            }

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