using System;

namespace Core.DomainModel.ItContract
{
    public class PaymentMilestone : Entity
    {
        public string Title { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        
        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItContract != null && ItContract.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}