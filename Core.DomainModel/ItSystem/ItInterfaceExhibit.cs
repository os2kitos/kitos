namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="DomainModel.ItSystem.ItInterface"/>
    /// an <see cref="ItSystem"/> exhibts (udstiller).
    /// This is a (sys) 1:M (inf) relation.
    /// </summary>
    public class ItInterfaceExhibit : Entity
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
    }
}
