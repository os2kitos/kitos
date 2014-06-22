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
    }
}