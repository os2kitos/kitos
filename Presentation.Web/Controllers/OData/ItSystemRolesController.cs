using System.Linq;
using System.Web.Http;
using System.Web.OData;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItSystemRolesController : BaseEntityController<ItSystemRole>
    {
        public ItSystemRolesController(IGenericRepository<ItSystemRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable().Where(x => x.IsActive && !x.IsSuggestion));
        }
    }
}
