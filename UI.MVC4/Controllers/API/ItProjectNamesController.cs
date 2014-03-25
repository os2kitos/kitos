using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectNamesController : GenericOptionApiController<ItProjectModuleName, Config, OptionDTO>
   {
        public ItProjectNamesController(IGenericRepository<ItProjectModuleName> repository) 
            : base(repository)
        {
        }
   }
}