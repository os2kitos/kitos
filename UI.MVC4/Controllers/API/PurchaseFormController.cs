using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PurchaseFormController : GenericOptionApiController<PurchaseForm, ItContract, OptionDTO>
    {
        public PurchaseFormController(IGenericRepository<PurchaseForm> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<PurchaseForm> GetAllQuery(int skip, int take, bool descending, string orderBy = null)
        {
            var field = orderBy ?? "Id";
            return Repository.AsQueryable().Where(x => x.IsActive).OrderByField(field, descending).Skip(skip).Take(take);
        }
    }
}