using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalArchiveTypesController : LocalOptionBaseController<LocalArchiveType, ItSystemUsage, ArchiveType>
    {
        public LocalArchiveTypesController(IGenericRepository<LocalArchiveType> repository, IGenericRepository<ArchiveType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
