using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ItProjectRolesController : BaseOptionController<ItProjectRole, ItProjectRight>
    {
        public ItProjectRolesController(IGenericRepository<ItProjectRole> repository)
            : base(repository)
        {
        }
    }
}
