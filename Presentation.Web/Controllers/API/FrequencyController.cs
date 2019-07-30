using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class FrequencyController : GenericOptionApiController<FrequencyType, DataRowUsage, OptionDTO>
    {
        public FrequencyController(IGenericRepository<FrequencyType> repository) : base(repository)
        {
        }
    }
}
