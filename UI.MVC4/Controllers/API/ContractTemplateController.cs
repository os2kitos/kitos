using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class ContractTemplateController : GenericApiController<ContractTemplate, int>
    {
        public ContractTemplateController(IGenericRepository<ContractTemplate> repository) 
            : base(repository)
        {
        }
    }
}