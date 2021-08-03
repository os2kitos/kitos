using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class SensitiveDataTypesController : BaseOptionController<SensitiveDataType, ItSystemUsage>
    {
        public SensitiveDataTypesController(IGenericRepository<SensitiveDataType> repository)
            : base(repository)
        {
        }
    }
}