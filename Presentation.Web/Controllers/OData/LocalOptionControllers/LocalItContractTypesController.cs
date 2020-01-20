using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItContractTypesController : LocalOptionBaseController<LocalItContractType, ItContract, ItContractType>
    {
        public LocalItContractTypesController(IGenericRepository<LocalItContractType> repository, IGenericRepository<ItContractType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
