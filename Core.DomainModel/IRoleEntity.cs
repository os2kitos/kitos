namespace Core.DomainModel
{
    /// <summary>
    /// Base entity for a role. Notice that the role entity does NOT specific which user has that role,
    /// nor what is the object of the role. This is done using the <see><cref>IRight</cref></see>.
    /// </summary>
    public interface IRoleEntity
    {
        bool HasReadAccess { get; set; }
        bool HasWriteAccess { get; set; }
    }
}
