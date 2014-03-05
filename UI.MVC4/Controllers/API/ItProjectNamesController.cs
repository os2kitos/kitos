using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectNamesController : GenericOptionApiController<ItProjectModuleName, Config>
   {
        public ItProjectNamesController(IGenericRepository<ItProjectModuleName> repository) 
            : base(repository)
        {
        }
   }
}