using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class ItSystemRoleController : GenericOptionApiController<ItSystemRole, ItSystemRight, RoleDTO>
    {
        public ItSystemRoleController(IGenericRepository<ItSystemRole> repository) 
            : base(repository)
        {
        }
    }
}
