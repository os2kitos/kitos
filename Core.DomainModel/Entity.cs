namespace Core.DomainModel
{
    /// <summary>
    /// Base entity class.
    /// Every domain model should extend this
    /// </summary>
    public abstract class Entity
    {
        public int Id { get; set; }

        public int? ObjectOwnerId { get; set; }
        public virtual User ObjectOwner { get; set; }

        public virtual bool HasUserWriteAccess(User user)
        {
            return ObjectOwnerId == user.Id || user.IsGlobalAdmin;
        }
    }
}