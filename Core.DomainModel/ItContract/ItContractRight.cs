namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// Associates a <see cref="User"/> with an it contract (<see cref="Object"/>) in a specific <see cref="Role"/>.
    /// </summary>
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