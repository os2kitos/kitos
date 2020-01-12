using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ItProjectTypesController : BaseOptionController<ItProjectType, ItProject>
    {
        public ItProjectTypesController(IGenericRepository<ItProjectType> repository)
            : base(repository)
        {
        }
    }
}