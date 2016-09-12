using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices;
using System.Web.Http.Results;
using System.Net;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitsController : BaseEntityController<OrganizationUnit>
    {
        public OrganizationUnitsController(IGenericRepository<OrganizationUnit> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        [ODataRoute("OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits()
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            var result = Repository.AsQueryable().Where(ou => ou.OrganizationId == loggedIntoOrgId);
            return Ok(result);
        }

        //GET /OrganizationUnits(1)
        [EnableQuery]
        [ODataRoute("OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int unitKey)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            var result = Repository.GetByKey(unitKey);
            if (loggedIntoOrgId != result.OrganizationId && !AuthenticationService.HasReadAccessOutsideContext(UserId))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            return Ok(result);
        }

        //GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int orgKey)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != orgKey && !AuthenticationService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
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

            if (AuthenticationService.HasReadAccess(CurentUser, entity))
                return Ok(entity);

            return new StatusCodeResult(HttpStatusCode.Forbidden, this);
        }
    }
}
