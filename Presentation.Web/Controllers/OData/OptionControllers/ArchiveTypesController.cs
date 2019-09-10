using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ArchiveTypesController : BaseOptionController<ArchiveType, ItSystemUsage>
    {
        public ArchiveTypesController(IGenericRepository<ArchiveType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}