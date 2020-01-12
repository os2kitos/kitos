using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalPurchaseFormTypesController : LocalOptionBaseController<LocalPurchaseFormType, ItContract, PurchaseFormType>
    {
        public LocalPurchaseFormTypesController(IGenericRepository<LocalPurchaseFormType> repository, IAuthenticationService authService, IGenericRepository<PurchaseFormType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
