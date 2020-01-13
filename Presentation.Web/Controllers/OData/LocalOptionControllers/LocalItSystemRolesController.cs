using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItSystemRolesController : LocalOptionBaseController<LocalItSystemRole, ItSystemRight, ItSystemRole>
    {
        public LocalItSystemRolesController(IGenericRepository<LocalItSystemRole> repository, IGenericRepository<ItSystemRole> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
