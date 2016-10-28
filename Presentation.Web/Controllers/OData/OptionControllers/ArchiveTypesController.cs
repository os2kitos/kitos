using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ArchiveTypesController : BaseOptionController<ArchiveType, ItSystemUsage>
    {
        public ArchiveTypesController(IGenericRepository<ArchiveType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}