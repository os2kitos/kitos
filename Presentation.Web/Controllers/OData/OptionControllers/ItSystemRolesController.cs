using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ItSystemRolesController : BaseOptionController<ItSystemRole, ItSystemRight>
    {
        public ItSystemRolesController(IGenericRepository<ItSystemRole> repository)
            : base(repository)
        {
        }
    }
}
