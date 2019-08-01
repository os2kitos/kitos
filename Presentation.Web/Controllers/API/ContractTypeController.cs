using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ContractTypeController : GenericOptionApiController<ItContractType, ItContract, OptionDTO>
    {
        public ContractTypeController(IGenericRepository<ItContractType> repository) 
            : base(repository)
        {
        }
    }
}
