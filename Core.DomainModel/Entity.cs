namespace Core.DomainModel
{
    /// <summary>
    /// Base entity class.
    /// Every domain model should extend this.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Gets or sets the primary identifier.
        /// </summary>
        /// <value>
        /// The primary identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the object owner <see cref="User"/> identifier.
        /// </summary>
        /// <value>
        /// The object owner <see cref="User"/> identifier.
        /// </value>
        public int? ObjectOwnerId { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="User"/> that owns this instance.
        /// </summary>
        /// <value>
        /// The object owner.
        /// </value>
        public virtual User ObjectOwner { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        /// <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool HasUserWriteAccess(User user)
        {
            return ObjectOwnerId == user.Id || user.IsGlobalAdmin;
        }
    }
}