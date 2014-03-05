using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemNamesController : GenericOptionApiController<ItSystemModuleName, Config>
    {
        public ItSystemNamesController(IGenericRepository<ItSystemModuleName> repository) 
            : base(repository)
        {
        }
    }
}