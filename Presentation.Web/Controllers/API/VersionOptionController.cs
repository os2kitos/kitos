using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class VersionOptionController : GenericOptionApiController<VersionOption, ItInterface, OptionDTO>
    {
        public VersionOptionController(IGenericRepository<VersionOption> repository)
            : base(repository)
        {
        }
    }
}
