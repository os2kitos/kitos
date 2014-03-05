using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceTypeController : GenericOptionApiController<InterfaceType, ItSystem>
    {
        public InterfaceTypeController(IGenericRepository<InterfaceType> repository) 
            : base(repository)
        {
        }
    }
}