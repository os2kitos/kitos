using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ConfigController: GenericApiController<Configuration, int>
    {
        public ConfigController(IGenericRepository<Configuration> repository) 
            : base(repository)
        {
        }
    }
}