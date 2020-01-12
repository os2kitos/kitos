using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class FrequencyTypesController : BaseOptionController<FrequencyType, DataRowUsage>
    {
        public FrequencyTypesController(IGenericRepository<FrequencyType> repository)
            : base(repository)
        {
        }
    }
}