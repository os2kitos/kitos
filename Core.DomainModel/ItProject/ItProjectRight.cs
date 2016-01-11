namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// Associates a <see cref="User"/> with an it project (<see cref="Object"/>) in a specific <see cref="Role"/>.
    /// </summary>
    public class ItProjectRight : Entity, IRight<ItProject, ItProjectRight, ItProjectRole>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int ObjectId { get; set; }
        public virtual User User { get; set; }
        public virtual ItProjectRole Role { get; set; }
        public virtual ItProject Object { get; set; }
    }
}
