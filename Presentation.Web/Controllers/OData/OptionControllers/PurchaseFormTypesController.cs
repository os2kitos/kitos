using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class PurchaseFormTypesController : BaseOptionController<PurchaseFormType, ItContract>
    {
        public PurchaseFormTypesController(IGenericRepository<PurchaseFormType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}