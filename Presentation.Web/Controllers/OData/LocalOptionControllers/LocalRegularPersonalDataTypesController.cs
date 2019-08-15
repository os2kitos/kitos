using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class LocalRegularPersonalDataTypesController : LocalOptionBaseController<LocalRegularPersonalDataType, ItSystem, RegularPersonalDataType>
    {
        public LocalRegularPersonalDataTypesController(IGenericRepository<LocalRegularPersonalDataType> repository, IAuthenticationService authService, IGenericRepository<RegularPersonalDataType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}