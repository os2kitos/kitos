using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalRegularPersonalDataTypesController : LocalOptionBaseController<LocalRegularPersonalDataType, ItSystem, RegularPersonalDataType>
    {
        public LocalRegularPersonalDataTypesController(IGenericRepository<LocalRegularPersonalDataType> repository, IGenericRepository<RegularPersonalDataType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}