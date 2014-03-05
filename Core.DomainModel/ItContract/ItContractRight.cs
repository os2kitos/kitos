namespace Core.DomainModel.ItContract
{
    public class ItContractRight : IRight<ItContract, ItContractRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public ItContractRole Role { get; set; }
        public ItContract Object { get; set; }
    }
}