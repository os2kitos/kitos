using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Net;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemsController : BaseEntityController<ItSystem>
    {
        private readonly IAuthenticationService _authService;

        public ItSystemsController(IGenericRepository<ItSystem> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _authService = authService;
        }
        
        [ODataRoute("ItSystems")]
        public override IHttpActionResult Get()
        {
            return base.Get();
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystems")]
        public IHttpActionResult GetItSystems(int key)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return StatusCode(HttpStatusCode.Forbidden);

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
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (!_authService.HasReadAccessOutsideContext(UserId))
            {
                if (loggedIntoOrgId != key)
                    return StatusCode(HttpStatusCode.Forbidden);

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

            if (_authService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
