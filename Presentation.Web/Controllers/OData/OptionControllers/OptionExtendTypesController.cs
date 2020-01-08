using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class OptionExtendTypesController : BaseOptionController<OptionExtendType, ItContract>
    {
        public OptionExtendTypesController(IGenericRepository<OptionExtendType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}