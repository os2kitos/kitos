using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class MethodTypesController : BaseOptionController<MethodType, ItInterface>
    {
        public MethodTypesController(IGenericRepository<MethodType> repository)
            : base(repository)
        {
        }
    }
}