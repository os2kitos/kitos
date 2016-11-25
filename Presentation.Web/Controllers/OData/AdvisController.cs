using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainServices;
using Hangfire;
using System;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    public class AdvisController : BaseEntityController<Advice>
    {

        IAuthenticationService _authService;
        IAdviceService _adviceService;
        public AdvisController(IAdviceService adviceService, IGenericRepository<Advice> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _authService = authService;
            _adviceService = adviceService;
        }

        [EnableQuery]
        public override IHttpActionResult Post(Advice advice)
        {

            var response = base.Post(advice);

            if (response.GetType() == typeof(System.Web.OData.Results.CreatedODataResult<Advice>)) {
                var server = new BackgroundJobServer();
                try
                {
                    RecurringJob.AddOrUpdate(
                    () => _adviceService.sendAdvice(advice),
                    Cron.Monthly);
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

            if (response.GetType() == typeof(System.Web.OData.Results.UpdatedODataResult<Advice>))
            {
                //TODO UPDATE HANGFIRE SCHEDULE
            }
            
            return response;
        }

        [EnableQuery]
        [ODataRoute("GetAdvicesByObjectID(id={id},type={type})")]
        public IHttpActionResult GetByObjectID(int id,int type)
        {
            if (UserId == 0)
                return Unauthorized();

            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(Advice));

            if (_authService.HasReadAccessOutsideContext(UserId) || hasOrg == false)
                return Ok(Repository.AsQueryable());//.Where(x=> x.ObjectId == id && x.Type == (ObjectType)type));

            return Ok(Repository.AsQueryable());//.Where(x => ((IHasOrganization)x).OrganizationId == _authService.GetCurrentOrganizationId(UserId) && x.ObjectId == id && x.Type == (ObjectType)type));
        }

        [EnableQuery]
        public IHttpActionResult Delete(int ObjectId)
        {
            var response = base.Delete(ObjectId);

            if (response.GetType() == typeof(StatusCodeResult))
            {
                //TODO Delete HANGFIRE SCHEDULE
            }
            return response;
        }



    }
}