using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemRightsController : BaseEntityController<ItSystemRight>
    {
        public ItSystemRightsController(IGenericRepository<ItSystemRight> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItSystemUsages({usageId})/Rights")]
        public IHttpActionResult GetItSystems(int orgId, int usageId)
        {
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == usageId);
            return Ok(result);
        }
    }
}
