using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class BusinessTypesController : BaseOptionController<BusinessType, ItSystem>
    {
        public BusinessTypesController(IGenericRepository<BusinessType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}