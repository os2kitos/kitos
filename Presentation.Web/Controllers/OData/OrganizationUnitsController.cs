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
        [ODataRoute("Organizations({orgKey})/OrganizationUnits")]
        public IHttpActionResult GetOrganizationUnits(int orgKey)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey);
            return Ok(result);
        }

        // GET /Organizations(1)/OrganizationUnits
        [EnableQuery]
        [ODataRoute("Organizations({orgKey})/OrganizationUnits({unitKey})")]
        public IHttpActionResult GetOrganizationUnit(int orgKey, int unitKey)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == orgKey && m.Id == unitKey);
            return Ok(result);
        }
    }
}
