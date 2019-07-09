using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItContractTypesController : BaseOptionController<ItContractType, ItContract>
    {
        public ItContractTypesController(IGenericRepository<ItContractType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}