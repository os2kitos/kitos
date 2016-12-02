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
                
                try
                {
                    RecurringJob.AddOrUpdate("Advice: " + createdRepsonse.Entity.Id,
                    () => _adviceService.sendAdvice(createdRepsonse.Entity.Id),
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