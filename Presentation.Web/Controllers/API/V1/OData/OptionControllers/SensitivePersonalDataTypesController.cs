using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class SensitivePersonalDataTypesController : BaseOptionController<SensitivePersonalDataType, ItSystem>
    {
        public SensitivePersonalDataTypesController(IGenericRepository<SensitivePersonalDataType> repository)
            : base(repository)
        {
        }
    }
}