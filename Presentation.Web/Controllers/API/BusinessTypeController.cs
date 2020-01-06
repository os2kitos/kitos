using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class BusinessTypeController : GenericOptionApiController<BusinessType, ItSystem, OptionDTO>
    {
        public BusinessTypeController(IGenericRepository<BusinessType> repository) : base(repository)
        {
        }
    }
}
