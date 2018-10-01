using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainModel.ItSystemUsage;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ArchiveTypesController : BaseOptionController<ArchiveType, ItSystemUsage>
    {
        public ArchiveTypesController(IGenericRepository<ArchiveType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}