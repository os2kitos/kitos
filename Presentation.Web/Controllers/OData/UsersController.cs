using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainServices;
using System.Web.OData.Routing;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    public class UsersController : BaseEntityController<User>
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public UsersController(IGenericRepository<User> repository, IUserService userService, IAuthenticationService authService) : base(repository)
        {
            _userService = userService;
            _authService = authService;
        }


        //GET /Organizations(1)/DefaultOrganizationForUsers
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/DefaultOrganizationForUsers")]
        public IHttpActionResult GetDefaultOrganizationForUsers(int orgKey)
        {
            var loggedIntoOrgId = _userService.GetCurrentOrganizationId(UserId);
            if (loggedIntoOrgId != orgKey && !_authService.HasReadAccessOutsideContext(UserId))
            {
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);
            }
            else
            {
                var result = Repository.AsQueryable().Where(m => m.DefaultOrganizationId == orgKey);
                return Ok(result);
            }
        }
    }
}