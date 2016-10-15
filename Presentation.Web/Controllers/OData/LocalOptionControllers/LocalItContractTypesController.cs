using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalItContractTypesController : LocalOptionBaseController<LocalItContractType, ItContract, ItContractType>
    {
        public LocalItContractTypesController(IGenericRepository<LocalItContractType> repository, IAuthenticationService authService, IGenericRepository<ItContractType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
