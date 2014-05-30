namespace Core.DomainModel
{
    public interface IRight<TObject, TRight, TRole>
        where TObject : Entity, IHasRights<TRight>
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