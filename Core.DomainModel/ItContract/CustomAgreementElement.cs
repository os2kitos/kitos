namespace Core.DomainModel.ItContract
{
    public class CustomAgreementElement : Entity
    {
        public string Name { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItContract != null && ItContract.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}