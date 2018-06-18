using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Hangfire;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Results;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    using Core.DomainModel.AdviceSent;
    using System.Net;

    public class AdviceController : BaseEntityController<Advice>
    {

        IAuthenticationService _authService;
        IAdviceService _adviceService;
        IGenericRepository<Advice> _repository;
        IGenericRepository<AdviceSent> _sentRepository;

        public AdviceController(IAdviceService adviceService, IGenericRepository<Advice> repository, IAuthenticationService authService, IGenericRepository<AdviceSent> sentRepository)
            : base(repository, authService)
        {
            _authService = authService;
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
                    //todo log error
                    return InternalServerError(e);
                }

                try
                {
                   switch (advice.Scheduling) {
                            case Scheduling.Immediate:
                            var jobId = BackgroundJob.Enqueue(
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id));
                                break;
                            case Scheduling.Hour:
                            
                            string cron = "0 * * * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Day:

                             var month = advice.AlarmDate.Value.Month;
                             var day = advice.AlarmDate.Value.Day;
                             cron = "0 8 * * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Week:
                            string weekDay = advice.AlarmDate.Value.DayOfWeek.ToString().Substring(0, 3);
                            cron = "0 8 *  * " + weekDay;

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Month:

                            day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " * *";

                            RecurringJob.AddOrUpdate(name,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
                    cron);
                                break;
                            case Scheduling.Year:

                             month = advice.AlarmDate.Value.Month;
                             day = advice.AlarmDate.Value.Day;
                             cron = "0 8 " + day + " " + month + " *";
                                
                                RecurringJob.AddOrUpdate(name,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
                    cron);
                            break;
                        }
                }
                catch (Exception e) {
                    //todo log error
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
                    () => _adviceService.sendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Day:
                            
                            cron = "0 8 * * *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.sendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Week:
                            string weekDay = advice.AlarmDate.Value.DayOfWeek.ToString().Substring(0, 3);
                            cron = "0 8 *  * " + weekDay;

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.sendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Month:

                            var day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " * *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                    () => _adviceService.sendAdvice(key),
                    cron);
                            break;
                        case Scheduling.Year:

                            var month = advice.AlarmDate.Value.Month;
                            day = advice.AlarmDate.Value.Day;
                            cron = "0 8 " + day + " " + month + " *";

                            RecurringJob.AddOrUpdate(advice.JobId,
                () => _adviceService.sendAdvice(key),
                cron);
                            break;
                    }
                }
                catch (Exception e)
                {
                    //todo log error
                    return InternalServerError(e);
                }
            }
            
            return response;
        }

        [EnableQuery]
        public IHttpActionResult GetAdvicesByObjectID(int id,ObjectType type)
        {
            if (UserId == 0)
                return Unauthorized();

            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(Advice));

            if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                return Ok(Repository.AsQueryable().Where(x=> x.RelationId == id && x.Type == type));

            return Ok(Repository.AsQueryable()
                    .Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) && x.RelationId == id && x.Type == type));
        }

        [EnableQuery]
        public IHttpActionResult GetByOrganization([FromODataUri]int orgKey)
        {
            if (UserId == 0)
                return Unauthorized();

            var currentOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (orgKey != currentOrgId)
                return Unauthorized();

            var result = _adviceService.GetAdvicesForOrg(orgKey);

            return Ok(result.AsQueryable());
        }

        [EnableQuery]
        public override IHttpActionResult Delete(int key)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == key);
            if (entity == null)
                return NotFound();

            var anySents = _sentRepository.AsQueryable().Any(m => m.AdviceId == key);

            if (anySents) {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            if (!_authService.HasWriteAccess(UserId, entity))
                return StatusCode(HttpStatusCode.Forbidden);


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
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}