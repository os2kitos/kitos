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
    public class AdviceController : BaseEntityController<Advice>
    {

        IAuthenticationService _authService;
        IAdviceService _adviceService;
        public AdviceController(IAdviceService adviceService, IGenericRepository<Advice> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _authService = authService;
            _adviceService = adviceService;
        }

        [EnableQuery]
        public override IHttpActionResult Post(Advice advice)
        {

            var response = base.Post(advice);

            if (response.GetType() == typeof(CreatedODataResult<Advice>)) {
                
                var createdRepsonse = (CreatedODataResult<Advice>)response ;
                var name = "Advice: " + createdRepsonse.Entity.Id;

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
                             cron = "0 8 * " + day + " *";

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
                    RecurringJob.AddOrUpdate("Advice: " + key,
                    () => _adviceService.sendAdvice(key),
                    Cron.Minutely);
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
        [ODataRoute("GetAdvicesByObjectID(id={id},type={type})")]
        public IHttpActionResult GetByObjectID(int id,ObjectType type)
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
        public override IHttpActionResult Delete(int key)
        {
            var response = base.Delete(key);

            if (response.GetType() == typeof(StatusCodeResult))
            {
                try
                {
                    RecurringJob.RemoveIfExists("Advice: " + key);
                }
                catch (Exception e) {
                    return InternalServerError(e);
                }
            }
            return response;
        }



    }
}