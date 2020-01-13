using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItInterfaceTypesController : BaseOptionController<ItInterfaceType, ItInterface>
    {
        public ItInterfaceTypesController(IGenericRepository<ItInterfaceType> repository)
            : base(repository)
        {
        }
    }
}