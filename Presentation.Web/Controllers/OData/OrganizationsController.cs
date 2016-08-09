using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationsController : BaseController<Organization>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public OrganizationsController(IGenericRepository<Organization> repository, IUserService userService, IAuthenticationService authService)
            : base(repository)
        {
            _userService = userService;
            _authService = authService;
        }

        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/LastChangedByUser")]
        public virtual IHttpActionResult GetLastChangedByUser(int orgKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
            else
            {
                var result = Repository.GetByKey(orgKey).LastChangedByUser;
                return Ok(result);
            }
        }
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ObjectOwner")]
        public virtual IHttpActionResult GetObjectOwner(int orgKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }

            var result = Repository.GetByKey(orgKey).ObjectOwner;
            return Ok(result);
        }

        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/Type")]
        public virtual IHttpActionResult GetType(int orgKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }

            var result = Repository.GetByKey(orgKey).Type;
            return Ok(result);
        }
    }
}
