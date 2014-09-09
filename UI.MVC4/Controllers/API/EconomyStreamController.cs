using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class EconomyStreamController : GenericApiController<EconomyStream, EconomyStreamDTO>
    {
        public EconomyStreamController(IGenericRepository<EconomyStream> repository) : base(repository)
        {
        }
    }
}
