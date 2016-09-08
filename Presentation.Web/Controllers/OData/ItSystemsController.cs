using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Web.Http.Results;
using System.Net;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        public ItSystemsController(IGenericRepository<ItSystem> repository)
            : base(repository)
        {}

        [EnableQuery(MaxExpansionDepth = 5)]
        [ODataRoute("ItSystems")]
        public override IHttpActionResult Get()
        {
            return base.Get();
            //if (AuthenticationService.HasReadAccessOutsideContext(CurentUser))
            //    return base.Get();

            //var orgId = CurrentOrganizationId;
            //return Ok(Repository.AsQueryable().Where(x => x.OrganizationId == orgId));
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystems")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (!AuthenticationService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return new StatusCodeResult(HttpStatusCode.Forbidden, this);

                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
                return Ok(result);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/BelongingSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/BelongingSystems")]
        public IHttpActionResult GetBelongingSystems(int key)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (!AuthenticationService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return new StatusCodeResult(HttpStatusCode.Forbidden, this);

                var result = Repository.AsQueryable().Where(m => m.BelongsToId == key);
                return Ok(result);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == key || m.AccessModifier == AccessModifier.Public);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/ItSystems(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ItSystems({sysKey})")]
        public IHttpActionResult GetItSystems(int orgKey, int sysKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.Id == sysKey);
            if (entity == null)
                return NotFound();

            if (AuthenticationService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }
    }
}
