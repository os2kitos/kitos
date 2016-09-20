using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices;
using Core.DomainModel.ItContract;

namespace Presentation.Web.Controllers.OData
{
    public class ItContractRightsController : BaseController<ItContractRight>
    {
        public ItContractRightsController(IGenericRepository<ItContractRight> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItContracts(1)/Rights
        [EnableQuery]
        [ODataRoute("Organizations({orgId})/ItContracts({contractId})/Rights")]
        public IHttpActionResult GetByItContract(int orgId, int contractId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.Object.OrganizationId == orgId && x.ObjectId == contractId);
            return Ok(result);
        }

        // GET /Users(1)/ItContractRights
        [EnableQuery]
        [ODataRoute("Users({userId})/ItContractRights")]
        public IHttpActionResult GetByUser(int userId)
        {
            // TODO figure out how to check auth
            var result = Repository.AsQueryable().Where(x => x.UserId == userId);
            return Ok(result);
        }
    }
}
