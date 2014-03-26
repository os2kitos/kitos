using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSupportNamesController : GenericOptionApiController<ItSupportModuleName, Config, OptionDTO>
    {
        public ItSupportNamesController(IGenericRepository<ItSupportModuleName> repository) 
            : base(repository)
        {
        }
    }
}