using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ContractTemplateController : GenericOptionApiController<ContractTemplate, ItContract, OptionDTO>
    {
        public ContractTemplateController(IGenericRepository<ContractTemplate> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<ContractTemplate> GetAllQuery(int skip, int take, bool descending, string orderBy = null)
        {
            var field = orderBy ?? "Id";
            return Repository.AsQueryable().Where(x => x.IsActive).OrderByField(field, descending).Skip(skip).Take(take);
        }
    }
}