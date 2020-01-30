using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class SensistivePersonalDataTypesController : BaseOptionController<SensitivePersonalDataType, ItSystem>
    {
        public SensistivePersonalDataTypesController(IGenericRepository<SensitivePersonalDataType> repository)
            : base(repository)
        {
        }
    }
}