namespace Core.DomainModel.ItSystem
{
    public class ItSystemRight : IRight<ItSystem, ItSystemRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public User User { get; set; }
        public ItSystemRole Role { get; set; }
        public ItSystem Object { get; set; }
    }
}