namespace Core.DomainModel
{
    public interface ILocaleEntity<TOriginal>
    {
        int Municipality_Id { get; set; }
        int Original_Id { get; set; }
        string Name { get; set; }

        Organization Organization { get; set; }
        TOriginal Original { get; set; }
    }
}