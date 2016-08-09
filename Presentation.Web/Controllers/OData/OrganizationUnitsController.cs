using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainServices;
using System.Web.Http.Results;
using System.Net;
using Core.ApplicationServices;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitsController : BaseController<OrganizationUnit>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public OrganizationUnitsController(IGenericRepository<OrganizationUnit> repository, IUserService userService, IAuthenticationService authService)
            : base(repository)
        {
            _userService = userService;
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits()
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            var result = Repository.AsQueryable().Where(ou => ou.OrganizationId == loggedIntoOrgId);
            return Ok(result);
        }

        //GET /OrganizationUnits(1)
        [EnableQuery]
        [ODataRoute("OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int unitKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            var result = Repository.GetByKey(unitKey);
            if (loggedIntoOrgId != result.OrganizationId && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
            else
            {
                return Ok(result);
            }
        }

        //GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
       [ODataRoute("Organizations({orgKey})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int orgKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey);
                return Ok(result);
            }
        }

        // GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int orgKey, int unitKey)
        {
            var entity = Repository.AsQueryable().SingleOrDefault(m => m.OrganizationId == orgKey && m.Id == unitKey);
            if (entity == null)
                return NotFound();

            if (_authService.HasReadAccess(UserId, entity))
            {
                return Ok(entity);
            }
            else
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
        }
    }
}
