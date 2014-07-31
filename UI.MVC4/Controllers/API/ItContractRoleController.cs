using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractRoleController : GenericOptionApiController<ItContractRole, ItContractRight, RoleDTO>
    {
        public ItContractRoleController(IGenericRepository<ItContractRole> repository) 
            : base(repository)
        {
        }
    }
}
