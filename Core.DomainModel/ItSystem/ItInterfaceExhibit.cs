namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="DomainModel.ItSystem.ItInterface"/>
    /// an <see cref="ItSystem"/> exhibts (udstiller).
    /// This is a (sys) 1:M (inf) relation.
    /// </summary>
    public class ItInterfaceExhibit : Entity, IContextAware
    {
        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public virtual ItInterface ItInterface { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystem != null && ItSystem.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            if (ItSystem != null)
                return ItSystem.IsInContext(organizationId);

            return false;
        }
    }
}
