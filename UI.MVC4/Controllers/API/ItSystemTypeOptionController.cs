using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemTypeOptionController : GenericOptionApiController<ItSystemTypeOption, ItSystem, OptionDTO>
    {
        public ItSystemTypeOptionController(IGenericRepository<ItSystemTypeOption> repository) 
            : base(repository)
        {
        }
    }
}
