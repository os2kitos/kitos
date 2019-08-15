using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class FrequencyTypesController : BaseOptionController<FrequencyType, DataRowUsage>
    {
        public FrequencyTypesController(IGenericRepository<FrequencyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}