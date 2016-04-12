using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ContractTemplateController : GenericOptionApiController<ContractTemplateType, ItContract, OptionDTO>
    {
        public ContractTemplateController(IGenericRepository<ContractTemplateType> repository) 
            : base(repository)
        {
        }
    }
}
