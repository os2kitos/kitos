using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ArchiveTypeController : GenericOptionApiController<ArchiveType, ItSystemUsage, OptionDTO>
    {
        public ArchiveTypeController(IGenericRepository<ArchiveType> repository) 
            : base(repository)
        {
        }
    }
}
