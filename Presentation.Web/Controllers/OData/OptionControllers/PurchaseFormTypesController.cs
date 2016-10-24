using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class PurchaseFormTypesController : BaseEntityController<PurchaseFormType>
    {
        public PurchaseFormTypesController(IGenericRepository<PurchaseFormType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}