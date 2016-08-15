using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItProjectRightsController : BaseEntityController<ItProjectRight>
    {
        public ItProjectRightsController(IGenericRepository<ItProjectRight> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItProjects({projId})/Rights")]
        public IHttpActionResult GetItSystems(int orgId, int projId)
        {
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == projId);
            return Ok(result);
        }
    }
}
