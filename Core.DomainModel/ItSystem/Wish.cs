namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// A user generated feature requests for an existing it system.
    /// </summary>
    public class Wish : Entity
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is public.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is public; otherwise, <c>false</c>.
        /// </value>
        public bool IsPublic { get; set; } // TODO shouldn't this be IHasAccessModifier
        /// <summary>
        /// Gets or sets the wish text.
        /// </summary>
        /// <value>
        /// The wish text.
        /// </value>
        public string Text { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// Gets or sets the user that made the wish.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public virtual User User { get; set; }
        public int ItSystemUsageId { get; set; }
        /// <summary>
        /// Gets or sets it system which this wish concerns.
        /// </summary>
        /// <remarks>
        /// Wishes are not associated with an it system directly,
        /// but instead the usage of an it system.
        /// </remarks>
        /// <value>
        /// It system usage.
        /// </value>
        public virtual ItSystemUsage ItSystemUsage { get; set; }
        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
