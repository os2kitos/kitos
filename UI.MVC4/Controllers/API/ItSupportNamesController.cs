using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItSupportNamesController : GenericOptionApiController<ItSupportModuleName, Config>
    {
        public ItSupportNamesController(IGenericRepository<ItSupportModuleName> repository) 
            : base(repository)
        {
        }
    }
}