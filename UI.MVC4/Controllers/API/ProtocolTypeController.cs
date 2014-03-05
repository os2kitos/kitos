using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ProtocolTypeController : GenericOptionApiController<ProtocolType, ItSystem>
    {
        public ProtocolTypeController(IGenericRepository<ProtocolType> repository) 
            : base(repository)
        {
        }
    }
}