using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class EnvironmentController : GenericOptionApiController<Environment, Technology>
    {
        public EnvironmentController(IGenericRepository<Environment> repository)
            : base(repository)
        {
        }
    }
}