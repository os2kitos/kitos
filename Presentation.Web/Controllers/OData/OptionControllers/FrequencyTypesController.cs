using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class FrequencyTypesController : BaseEntityController<FrequencyType>
    {
        public FrequencyTypesController(IGenericRepository<FrequencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}