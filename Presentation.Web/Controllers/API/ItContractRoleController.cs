using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItContractRoleController : GenericOptionApiController<ItContractRole, ItContractRight, RoleDTO>
    {
        public ItContractRoleController(IGenericRepository<ItContractRole> repository) 
            : base(repository)
        {
        }
    }
}
