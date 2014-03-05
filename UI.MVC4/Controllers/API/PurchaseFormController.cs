using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class PurchaseFormController : GenericOptionApiController<PurchaseForm, ItContract>
    {
        public PurchaseFormController(IGenericRepository<PurchaseForm> repository) 
            : base(repository)
        {
        }
    }
}