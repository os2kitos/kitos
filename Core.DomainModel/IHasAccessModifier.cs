namespace Core.DomainModel
{
    public enum AccessModifier
    {
        /// <summary>
        /// Only users within the organization this instance was created in have read access.
        /// </summary>
        Normal, 
        /// <summary>
        /// Everyone have read access.
        /// </summary>
        Public, 
        /// <summary>
        /// Only users with a role on the instance have read access.
        /// </summary>
        Private
    }

    /// <summary>
    /// Implemented by types whose read access can be modified
    /// </summary>
    public interface IHasAccessModifier
    {
        AccessModifier AccessModifier { get; set; }
    }
}
