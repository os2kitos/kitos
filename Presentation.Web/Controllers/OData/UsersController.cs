using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using Core.DomainModel;
using Core.DomainServices;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    public class UsersController : BaseEntityController<User>
    {

        public UsersController(IGenericRepository<User> repository) : base(repository)
        {
        }

        //GET /Organizations(1)/DefaultOrganizationForUsers
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/DefaultOrganizationForUsers")]
        public IHttpActionResult GetDefaultOrganizationForUsers(int orgKey)
        {
            var loggedIntoOrgId = CurrentOrganizationId;
            if (loggedIntoOrgId != orgKey && !AuthenticationService.HasReadAccessOutsideContext(UserId))
                return new StatusCodeResult(HttpStatusCode.Forbidden, this);

            var result = Repository.AsQueryable().Where(m => m.DefaultOrganizationId == orgKey);
            return Ok(result);
        }
    }
}