using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class PurchaseFormController : GenericApiController<PurchaseForm, int>
    {
        public PurchaseFormController(IGenericRepository<PurchaseForm> repository) 
            : base(repository)
        {
        }
    }
}