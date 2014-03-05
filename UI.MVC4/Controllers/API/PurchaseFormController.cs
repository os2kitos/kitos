using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class PurchaseFormController : GenericOptionApiController<PurchaseForm, ItContract>
    {
        public PurchaseFormController(IGenericRepository<PurchaseForm> repository) 
            : base(repository)
        {
        }

        protected override IEnumerable<PurchaseForm> GetAllQuery()
        {
            return Repository.Get(x => x.IsActive);
        }
    }
}