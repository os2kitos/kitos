using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItContractTemplateTypesController : BaseOptionController<ItContractTemplateType, ItContract>
    {
        public ItContractTemplateTypesController(IGenericRepository<ItContractTemplateType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}