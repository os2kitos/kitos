namespace Core.DomainModel
{
    public interface IRight<TObject, TRole> : IEntity<int>
        where TRole : IEntity<int>
        where TObject : IEntity<int>
    {
        int User_Id { get; set; }
        int Role_Id { get; set; }
        int Object_Id { get; set; }

        User User { get; set; }
        TRole Role { get; set; }
        TObject Object { get; set; }
    }
}