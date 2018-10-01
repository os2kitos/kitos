using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PurchaseFormTypesController : BaseOptionController<PurchaseFormType, ItContract>
    {
        public PurchaseFormTypesController(IGenericRepository<PurchaseFormType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}