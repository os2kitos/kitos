using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationsController : BaseEntityController<Organization>
    {
        private readonly IOrganizationService _organizationService;
        private readonly IAuthenticationService _authService;

        public OrganizationsController(IGenericRepository<Organization> repository, IOrganizationService organizationService, IAuthenticationService authService)
            : base(repository, authService)
        {
            _organizationService = organizationService;
            _authService = authService;
        }

        [ODataRoute("Organizations({orgKey})/RemoveUser")]
        public IHttpActionResult DeleteRemoveUserFromOrganization(int orgKey, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = 0;
            if (parameters.ContainsKey("userId"))
            {
                userId = (int)parameters["userId"];
                // TODO check if user is allowed to remove users from this organization
            }

            _organizationService.RemoveUser(orgKey, userId);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/LastChangedByUser")]
        public virtual IHttpActionResult GetLastChangedByUser(int orgKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
                return StatusCode(HttpStatusCode.Forbidden);

            var result = Repository.GetByKey(orgKey).LastChangedByUser;
            return Ok(result);
        }

        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/ObjectOwner")]
        public virtual IHttpActionResult GetObjectOwner(int orgKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var result = Repository.GetByKey(orgKey).ObjectOwner;
            return Ok(result);
        }

        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/Type")]
        public virtual IHttpActionResult GetType(int orgKey)
        {
            var loggedIntoOrgId = _authService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            var result = Repository.GetByKey(orgKey).Type;
            return Ok(result);
        }
    }
}
