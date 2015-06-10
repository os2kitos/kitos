using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitsController : BaseController<OrganizationUnit>
    {
        public OrganizationUnitsController(IGenericRepository<OrganizationUnit> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({key})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return Ok(result);
        }
    }
}
