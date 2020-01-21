using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [PublicApi]
    public class GlobalConfigsController : BaseEntityController<Core.DomainModel.GlobalConfig>
    {
        public GlobalConfigsController(IGenericRepository<Core.DomainModel.GlobalConfig> repository)
            : base(repository)
        {
        }
    }
}
