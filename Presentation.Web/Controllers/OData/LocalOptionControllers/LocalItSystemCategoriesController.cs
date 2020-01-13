using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.LocalOptions;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalItSystemCategoriesController : LocalOptionBaseController<LocalItSystemCategories, ItSystemUsage, ItSystemCategories>
    {
        public LocalItSystemCategoriesController(IGenericRepository<LocalItSystemCategories> repository, IGenericRepository<ItSystemCategories> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}