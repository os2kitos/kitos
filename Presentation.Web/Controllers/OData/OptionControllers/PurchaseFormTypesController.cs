using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class PurchaseFormTypesController : BaseOptionController<PurchaseFormType, ItContract>
    {
        public PurchaseFormTypesController(IGenericRepository<PurchaseFormType> repository)
            : base(repository)
        {
        }
    }
}