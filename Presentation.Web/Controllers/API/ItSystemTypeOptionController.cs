using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItSystemTypeOptionController : GenericOptionApiController<ItSystemType, ItSystem, OptionDTO>
    {
        public ItSystemTypeOptionController(IGenericRepository<ItSystemType> repository)
            : base(repository)
        {
        }
    }
}
