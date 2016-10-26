using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ItContractTypesController : BaseRoleController<ItContractType, ItContract>
    {
        public ItContractTypesController(IGenericRepository<ItContractType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}