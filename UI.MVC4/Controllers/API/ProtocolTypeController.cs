using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProtocolTypeController : GenericOptionApiController<ProtocolType, ItSystem, OptionDTO>
    {
        public ProtocolTypeController(IGenericRepository<ProtocolType> repository) 
            : base(repository)
        {
        }
    }
}