using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItProjectRightsController : BaseEntityController<ItProjectRight>
    {
        public ItProjectRightsController(IGenericRepository<ItProjectRight> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }

        // GET /Organizations(1)/ItProjects(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItProjects({projId})/Rights")]
        public IHttpActionResult GetByItProject(int orgId, int projId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == projId);
            return Ok(result);
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItProjectRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }
    }
}
