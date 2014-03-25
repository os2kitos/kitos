using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractNamesController : GenericOptionApiController<ItContractModuleName, Config, OptionDTO>
    {
        public ItContractNamesController(IGenericRepository<ItContractModuleName> repository) 
            : base(repository)
        {
        }
    }
}