using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class BusinessTypesController : BaseOptionController<BusinessType, ItSystem>
    {
        public BusinessTypesController(IGenericRepository<BusinessType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}