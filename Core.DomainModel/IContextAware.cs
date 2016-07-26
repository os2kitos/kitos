namespace Core.DomainModel
{
    /// <summary>
    /// Classes that implement this are aware of their organizational context.
    /// </summary>
    public interface IContextAware
    {
        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        bool IsInContext(int organizationId);
    }
}
