using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class ItInterfaceTypesController : BaseOptionController<ItInterfaceType, ItInterface>
    {
        public ItInterfaceTypesController(IGenericRepository<ItInterfaceType> repository)
            : base(repository)
        {
        }
    }
}