using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class TsaController : GenericOptionApiController<TsaType, ItInterface, OptionDTO>
    {
        public TsaController(IGenericRepository<TsaType> repository) : base(repository)
        {
        }
    }
}
