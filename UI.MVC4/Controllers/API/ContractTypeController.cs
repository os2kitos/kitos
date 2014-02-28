using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class ContractTypeController : GenericApiController<ContractType, int, ContractTypeDTO>
    {
        public ContractTypeController(IGenericRepository<ContractType> repository) 
            : base(repository)
        {
        }
    }
}