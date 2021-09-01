namespace Core.DomainModel
{
    /// <summary>
    /// Defines a three point relationsship between an user,
    /// an object and a role, to represents that the user has
    /// that role on that object.
    /// </summary>
    /// <typeparam name="TObject">Type of the object</typeparam>
    /// <typeparam name="TRight">Type of this</typeparam>
    /// <typeparam name="TRole">Type of the role</typeparam>
    public interface IRight<TObject, TRight, TRole>
        where TObject : HasRightsEntity<TObject, TRight, TRole>
        where TRight : IRight<TObject, TRight, TRole>
        where TRole : IRoleEntity, IHasId
    {
        int UserId { get; set; }
        User User { get; set; }

        int RoleId { get; set; }
        TRole Role { get; set; }

        int ObjectId { get; set; }
        TObject Object { get; set; }
    }
}