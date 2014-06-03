namespace Core.DomainModel
{
    public interface IRight<TObject, TRight, TRole>
        where TObject : HasRightsEntity<TObject, TRight, TRole>
        where TRight : IRight<TObject, TRight, TRole>
        where TRole : IRoleEntity<TRight>
    {
        int UserId { get; set; }
        int RoleId { get; set; }
        int ObjectId { get; set; }

        User User { get; set; }
        TRole Role { get; set; }
        TObject Object { get; set; }
    }
}