using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class SensitiveDataTypesController : BaseEntityController<SensitiveDataType>
    {
        public SensitiveDataTypesController(IGenericRepository<SensitiveDataType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}