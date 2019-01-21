using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class RegularPersonalDataTypesController : BaseOptionController<RegularPersonalDataType, ItSystem>
    {
        public RegularPersonalDataTypesController(IGenericRepository<RegularPersonalDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}