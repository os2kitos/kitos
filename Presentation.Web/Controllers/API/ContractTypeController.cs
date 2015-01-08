using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ContractTypeController : GenericOptionApiController<ContractType, ItContract, OptionDTO>
    {
        public ContractTypeController(IGenericRepository<ContractType> repository) 
            : base(repository)
        {
        }
    }
}
