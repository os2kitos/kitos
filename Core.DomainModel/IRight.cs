namespace Core.DomainModel
{
    public interface IRight<TObject, TRole>
        where TRole : IEntity<int>
        where TObject : IEntity<int>
    {
        int UserId { get; set; }
        int RoleId { get; set; }
        int ObjectId { get; set; }

        User User { get; set; }
        TRole Role { get; set; }
        TObject Object { get; set; }
    }
}