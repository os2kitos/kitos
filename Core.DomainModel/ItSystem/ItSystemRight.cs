namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Associates a <see cref="User"/> with an it system (<see cref="Object"/>) in a specific <see cref="Role"/>.
    /// </summary>
    public class ItSystemRight : Entity, IRight<ItSystemUsage.ItSystemUsage, ItSystemRight, ItSystemRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual ItSystemRole Role { get; set; }
        public virtual ItSystemUsage.ItSystemUsage Object { get; set; }
    }
}