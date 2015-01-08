using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class FrequencyController : GenericOptionApiController<Frequency, DataRowUsage, OptionDTO>
    {
        public FrequencyController(IGenericRepository<Frequency> repository) : base(repository)
        {
        }
    }
}
