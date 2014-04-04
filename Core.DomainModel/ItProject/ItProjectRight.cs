namespace Core.DomainModel.ItProject
{
    public class ItProjectRight : IRight<ItProject, ItProjectRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public User User { get; set; }
        public ItProjectRole Role { get; set; }
        public ItProject Object { get; set; }
    }
}