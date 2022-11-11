using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [PublicApi]
    public class ItSystemUsagesController : BaseEntityController<ItSystemUsage>
    {
        public ItSystemUsagesController(IGenericRepository<ItSystemUsage> repository)
            : base(repository)
        {
        }
    }
}
