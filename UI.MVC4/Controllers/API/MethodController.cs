using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class MethodController : GenericApiController<Method, int>
    {
        public MethodController(IGenericRepository<Method> repository) 
            : base(repository)
        {
        }
    }
}