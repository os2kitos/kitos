namespace Core.DomainModel.ItSystem
{
    public class ItSystemRight : IRight<ItSystem, ItSystemRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public ItSystemRole Role { get; set; }
        public ItSystem Object { get; set; }
    }
}