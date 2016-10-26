using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class PriceRegulationTypesController : BaseRoleController<PriceRegulationType, ItContract>
    {
        public PriceRegulationTypesController(IGenericRepository<PriceRegulationType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}