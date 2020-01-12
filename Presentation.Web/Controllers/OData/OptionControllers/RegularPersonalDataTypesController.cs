using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class RegularPersonalDataTypesController : BaseOptionController<RegularPersonalDataType, ItSystem>
    {
        public RegularPersonalDataTypesController(IGenericRepository<RegularPersonalDataType> repository)
            : base(repository)
        {
        }
    }
}