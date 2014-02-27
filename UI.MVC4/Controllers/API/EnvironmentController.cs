using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class EnvironmentController : GenericApiController<Environment, int>
    {
        public EnvironmentController(IGenericRepository<Environment> repository) 
            : base(repository)
        {
        }
    }
}