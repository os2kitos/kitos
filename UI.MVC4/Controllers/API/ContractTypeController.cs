using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ContractTypeController : GenericApiController<ContractType, int>
    {
        public ContractTypeController(IGenericRepository<ContractType> repository) 
            : base(repository)
        {
        }
    }
}