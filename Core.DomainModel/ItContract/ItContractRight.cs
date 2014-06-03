namespace Core.DomainModel.ItContract
{
    public class ItContractRight : Entity, IRight<ItContract, ItContractRight, ItContractRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual ItContractRole Role { get; set; }
        public virtual ItContract Object { get; set; }
    }
}