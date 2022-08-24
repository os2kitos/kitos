namespace Core.DomainModel
{
    public interface ISupportsUserSpecificAccessControl
    {
        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        bool HasUserWriteAccess(User user);
    }
}
