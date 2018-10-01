using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalPurchaseFormTypesController : LocalOptionBaseController<LocalPurchaseFormType, ItContract, PurchaseFormType>
    {
        public LocalPurchaseFormTypesController(IGenericRepository<LocalPurchaseFormType> repository, IAuthenticationService authService, IGenericRepository<PurchaseFormType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
