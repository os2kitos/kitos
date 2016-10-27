using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ArchiveTypesController : BaseRoleController<ArchiveType, ItSystemUsage>
    {
        public ArchiveTypesController(IGenericRepository<ArchiveType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}