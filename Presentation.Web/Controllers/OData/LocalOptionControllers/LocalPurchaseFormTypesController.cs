using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalPurchaseFormTypesController : LocalOptionBaseController<LocalPurchaseFormType, ItContract, PurchaseFormType>
    {
        public LocalPurchaseFormTypesController(IGenericRepository<LocalPurchaseFormType> repository, IGenericRepository<PurchaseFormType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
