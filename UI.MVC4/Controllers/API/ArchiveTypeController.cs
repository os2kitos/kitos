using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ArchiveTypeController : GenericOptionApiController<ArchiveType, ItSystemUsage, OptionDTO>
    {
        public ArchiveTypeController(IGenericRepository<ArchiveType> repository) 
            : base(repository)
        {
        }
    }
}