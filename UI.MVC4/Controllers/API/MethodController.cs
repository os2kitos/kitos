using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class MethodController : GenericOptionApiController<Method,Interface>
    {
        public MethodController(IGenericRepository<Method> repository) 
            : base(repository)
        {
        }
    }
}