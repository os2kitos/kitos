using Core.DomainServices;
using Core.DomainModel.Organization;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Web.Http;
using System.Linq;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitRightsController : BaseController<OrganizationUnitRight>
    {
        public OrganizationUnitRightsController(IGenericRepository<OrganizationUnitRight> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItContracts(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/OrganizationUnits({orgUnitId})/Rights")]
        public IHttpActionResult GetByOrganizationUnit(int orgId, int orgUnitId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == orgUnitId);
            return Ok(result);
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/OrganizationUnitRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }
    }
}
