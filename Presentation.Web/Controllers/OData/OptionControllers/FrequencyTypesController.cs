using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class FrequencyTypesController : BaseRoleController<FrequencyType,DataRowUsage>
    {
        public FrequencyTypesController(IGenericRepository<FrequencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}