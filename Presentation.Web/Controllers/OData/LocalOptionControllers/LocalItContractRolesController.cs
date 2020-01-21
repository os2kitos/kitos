using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItContractRolesController : LocalOptionBaseController<LocalItContractRole, ItContractRight, ItContractRole>
    {
        public LocalItContractRolesController(IGenericRepository<LocalItContractRole> repository, IGenericRepository<ItContractRole> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
