using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ConfigController : GenericApiController<Config, ConfigDTO>
    {
        public ConfigController(IGenericRepository<Config> repository) 
            : base(repository)
        {
        }
        
    }
}
