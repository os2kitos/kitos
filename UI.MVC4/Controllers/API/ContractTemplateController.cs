using System.Collections.Generic;
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

        protected override IEnumerable<ContractTemplate> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}