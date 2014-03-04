using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class ContractTemplateController : GenericApiController<ContractTemplate, int, ContractTemplateDTO>
    {
        public ContractTemplateController(IGenericRepository<ContractTemplate> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ContractTemplate> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}