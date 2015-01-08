using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class TsaController : GenericOptionApiController<Tsa, ItInterface, OptionDTO>
    {
        public TsaController(IGenericRepository<Tsa> repository) : base(repository)
        {
        }
    }
}
