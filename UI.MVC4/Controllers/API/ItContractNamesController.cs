using Core.DomainModel;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItContractNamesController : GenericOptionApiController<ItContractModuleName, Config>
    {
        public ItContractNamesController(IGenericRepository<ItContractModuleName> repository) 
            : base(repository)
        {
        }
    }
}