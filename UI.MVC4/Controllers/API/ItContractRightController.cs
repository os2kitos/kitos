using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace UI.MVC4.Controllers.API
{
    public class ItContractRightController : GenericRightController<ItContractRight, ItContract, ItContractRole>
    {
        public ItContractRightController(IGenericRepository<ItContractRight> rightRepository, 
            IGenericRepository<ItContract> contractRepository) : base(rightRepository, contractRepository)
        {
        }
    }
}
