using System.Linq;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfacesController : BaseController<ItInterface>
    {
        public ItInterfacesController(IGenericRepository<ItInterface> repository)
            : base(repository)
        {
        }

        // HACK to enable DISTINCT query
        // When OData support it this should be removed!
        public IHttpActionResult Get(string filter, bool distinct)
        {
            return Ok(Repository.AsQueryable().GroupBy(x => x.Version).Select(grp => grp.Key).Where(x => x.StartsWith(filter)));
        }
    }
}
