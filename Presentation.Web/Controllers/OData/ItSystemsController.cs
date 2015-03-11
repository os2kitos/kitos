using System.Linq;
using System.Web.Http.OData;
using System.Web.OData.Routing;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemsController : BaseController<ItSystem>
    {
        public ItSystemsController(IGenericRepository<ItSystem> repository)
            : base(repository)
        {
        }

        // GET /Organizations(1)/ItSystems
        [EnableQuery]
        [ODataRoute("Organizations({key})/ItSystems")]
        public IQueryable<ItSystem> GetItSystems(int key)
        {
            var result = Repository.AsQueryable().Where(m => m.OrganizationId == key);
            return result;
        }
    }
}
