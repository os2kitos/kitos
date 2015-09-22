using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class EconomyStreamController : GenericContextAwareApiController<EconomyStream, EconomyStreamDTO>
    {
        public EconomyStreamController(IGenericRepository<EconomyStream> repository) : base(repository)
        {
        }
    }
}
