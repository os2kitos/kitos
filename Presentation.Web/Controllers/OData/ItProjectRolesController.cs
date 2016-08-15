using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItProjectRolesController : BaseEntityController<ItProjectRole>
    {
        public ItProjectRolesController(IGenericRepository<ItProjectRole> repository)
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
