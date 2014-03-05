using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ContractTypeController : GenericOptionApiController<ContractType, ItContract>
    {
        public ContractTypeController(IGenericRepository<ContractType> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ContractType> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}