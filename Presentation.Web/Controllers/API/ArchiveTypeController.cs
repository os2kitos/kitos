using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class ArchiveTypeController : GenericOptionApiController<ArchiveType, ItSystemUsage, OptionDTO>
    {
        public ArchiveTypeController(IGenericRepository<ArchiveType> repository) 
            : base(repository)
        {
        }
    }
}
