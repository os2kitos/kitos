using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemUsagesController : BaseController<ItSystemUsage>
    {
        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItSystemUsages
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystemUsages")]
        public IHttpActionResult GetItSystems(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }
    }
}
