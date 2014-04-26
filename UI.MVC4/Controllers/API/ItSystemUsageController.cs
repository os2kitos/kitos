using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageController : GenericApiController<ItSystemUsage, int, ItSystemUsageDTO> 
    {
        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository) 
            : base(repository)
        {
        }
    }
}
