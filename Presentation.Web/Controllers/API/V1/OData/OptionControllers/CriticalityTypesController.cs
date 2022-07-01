using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class CriticalityTypesController : BaseOptionController<CriticalityType, ItContract>
    {
        public CriticalityTypesController(IGenericRepository<CriticalityType> repository)
            : base(repository)
        {
        }
    }
}