using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItContractController : GenericApiController<ItContract, int, ItContractDTO>
    {
        public ItContractController(IGenericRepository<ItContract> repository) 
            : base(repository)
        {
        }
    }
}