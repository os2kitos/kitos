using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemNamesController : GenericOptionApiController<ItSystemModuleName, Config, OptionDTO>
    {
        public ItSystemNamesController(IGenericRepository<ItSystemModuleName> repository) 
            : base(repository)
        {
        }
    }
}