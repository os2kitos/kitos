using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItSystemCategoriesController : BaseOptionController<ItSystemCategories, ItSystemUsage>
    {
        public ItSystemCategoriesController(IGenericRepository<ItSystemCategories> repository)
            : base(repository)
        {

        }
    }
}