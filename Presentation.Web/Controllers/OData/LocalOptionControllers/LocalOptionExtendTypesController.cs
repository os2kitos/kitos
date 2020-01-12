using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalOptionExtendTypesController : LocalOptionBaseController<LocalOptionExtendType, ItContract, OptionExtendType>
    {
        public LocalOptionExtendTypesController(IGenericRepository<LocalOptionExtendType> repository, IAuthenticationService authService, IGenericRepository<OptionExtendType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
