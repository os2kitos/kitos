using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TsaController : GenericOptionApiController<Tsa, ItInterface, OptionDTO>
    {
        public TsaController(IGenericRepository<Tsa> repository) : base(repository)
        {
        }
    }
}
