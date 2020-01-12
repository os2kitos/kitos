using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.LocalOptions;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalItSystemCategoriesController : LocalOptionBaseController<LocalItSystemCategories, ItSystemUsage, ItSystemCategories>
    {
        public LocalItSystemCategoriesController(IGenericRepository<LocalItSystemCategories> repository, IAuthenticationService authService, IGenericRepository<ItSystemCategories> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}