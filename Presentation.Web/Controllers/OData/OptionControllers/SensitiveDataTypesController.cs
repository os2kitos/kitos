using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [DeprecatedApi]
    public class SensitiveDataTypesController : BaseOptionController<SensitiveDataType, ItSystemUsage>
    {
        public SensitiveDataTypesController(IGenericRepository<SensitiveDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}