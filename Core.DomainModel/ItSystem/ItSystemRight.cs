namespace Core.DomainModel.ItSystem
{
    public class ItSystemRight : Entity, IRight<ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual ItSystemRole Role { get; set; }
        public virtual ItSystemUsage Object { get; set; }
    }
}