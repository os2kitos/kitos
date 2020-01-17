using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItContractRolesController : BaseOptionController<ItContractRole, ItContractRight>
    {
        public ItContractRolesController(IGenericRepository<ItContractRole> repository)
            : base(repository)
        {
        }
    }
}
