using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class SensitiveDataTypesController : BaseOptionController<SensitiveDataType, ItSystemUsage>
    {
        public SensitiveDataTypesController(IGenericRepository<SensitiveDataType> repository)
            : base(repository)
        {
        }
    }
}