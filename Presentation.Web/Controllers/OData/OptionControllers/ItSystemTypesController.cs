using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItSystemTypesController : BaseOptionController<ItSystemType, ItSystem>
    {
        public ItSystemTypesController(IGenericRepository<ItSystemType> repository)
            : base(repository)
        {
        }
    }
}