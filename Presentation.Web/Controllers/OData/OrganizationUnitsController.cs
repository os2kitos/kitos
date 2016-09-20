using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices;
using System.Net;
using Core.DomainModel.Organization;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitsController : BaseEntityController<OrganizationUnit>
    {
        private readonly IAuthenticationService _authService;

        public OrganizationUnitsController(IGenericRepository<OrganizationUnit> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits()
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            var result = Repository.AsQueryable().Where(ou => ou.OrganizationId == loggedIntoOrgId);
            return Ok(result);
        }

        //GET /OrganizationUnits(1)
        [EnableQuery]
        [ODataRoute("OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int unitKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            var result = Repository.GetByKey(unitKey);
            if (loggedIntoOrgId != result.OrganizationId && !_authService.HasReadAccessOutsideContext(UserId))
                return StatusCode(HttpStatusCode.Forbidden);

            return Ok(result);
        }

        //GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int orgKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey);
            return Ok(result);
        }

        // GET /Organizations(1)/OrganizationUnits(1)
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int orgKey, int unitKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.OrganizationId == orgKey && m.Id == unitKey);
            if (entity == null)
                return NotFound();

            if (_authService.HasReadAccess(UserId, entity))
                return Ok(entity);

            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
