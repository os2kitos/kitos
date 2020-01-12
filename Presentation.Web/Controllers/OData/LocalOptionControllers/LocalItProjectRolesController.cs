using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalItProjectRolesController : LocalOptionBaseController<LocalItProjectRole, ItProjectRight, ItProjectRole>
    {
        public LocalItProjectRolesController(IGenericRepository<LocalItProjectRole> repository, IGenericRepository<ItProjectRole> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
