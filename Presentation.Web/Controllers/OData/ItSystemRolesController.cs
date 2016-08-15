using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemRolesController : BaseEntityController<ItSystemRole>
    {
        public ItSystemRolesController(IGenericRepository<ItSystemRole> repository)
            : base(repository)
        {
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable().Where(x => x.IsActive && !x.IsSuggestion));
        }
    }
}
