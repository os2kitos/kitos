using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        public ItSystemRightsController(IGenericRepository<ItSystemRight> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItSystemUsages({usageId})/Rights")]
        public IHttpActionResult GetByItSystem(int orgId, int usageId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == usageId);
            return Ok(result);
        }

        // GET /Users(1)/ItProjectRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItSystemRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }
    }
}
