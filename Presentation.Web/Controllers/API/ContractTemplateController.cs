using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class ContractTemplateController : GenericOptionApiController<ItContractTemplateType, ItContract, OptionDTO>
    {
        public ContractTemplateController(IGenericRepository<ItContractTemplateType> repository)
            : base(repository)
        {
        }
    }
}
