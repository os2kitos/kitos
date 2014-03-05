using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ContractTemplateController : GenericOptionApiController<ContractTemplate, ItContract>
    {
        public ContractTemplateController(IGenericRepository<ContractTemplate> repository) 
            : base(repository)
        {
        }
    }
}