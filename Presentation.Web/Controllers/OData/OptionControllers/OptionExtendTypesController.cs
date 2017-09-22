using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class OptionExtendTypesController : BaseOptionController<OptionExtendType, ItContract>
    {
        public OptionExtendTypesController(IGenericRepository<OptionExtendType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}