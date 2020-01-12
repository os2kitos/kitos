using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class InterfaceTypesController : BaseOptionController<InterfaceType, ItInterface>
    {
        public InterfaceTypesController(IGenericRepository<InterfaceType> repository)
            : base(repository)
        {
        }
    }
}