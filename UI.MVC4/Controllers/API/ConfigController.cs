using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ConfigController : GenericApiController<Config, ConfigDTO>
    {
        public ConfigController(IGenericRepository<Config> repository) 
            : base(repository)
        {
        }
        
    }
}