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

        protected override bool HasWriteAccess(ItContract theObject, Core.DomainModel.User user)
        {
            //contract signer also has write access
            if (theObject.ContractSignerId.GetValueOrDefault() == user.Id) return true;

            return base.HasWriteAccess(theObject, user);
        }

    }
}
