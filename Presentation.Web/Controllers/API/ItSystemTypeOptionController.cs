using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItSystemTypeOptionController : GenericOptionApiController<ItSystemTypeOption, ItSystem, OptionDTO>
    {
        public ItSystemTypeOptionController(IGenericRepository<ItSystemTypeOption> repository) 
            : base(repository)
        {
        }
    }
}
