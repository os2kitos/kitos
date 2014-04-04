namespace Core.DomainModel.ItContract
{
    public class ItContractRight : IRight<ItContract, ItContractRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public User User { get; set; }
        public ItContractRole Role { get; set; }
        public ItContract Object { get; set; }
    }
}