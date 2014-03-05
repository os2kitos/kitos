namespace Core.DomainModel.ItProject
{
    public class ItProjectRight : IRight<ItProject, ItProjectRole>
    {
        public int User_Id { get; set; }
        public int Role_Id { get; set; }
        public int Object_Id { get; set; }
        public User User { get; set; }
        public ItProjectRole Role { get; set; }
        public ItProject Object { get; set; }
    }
}