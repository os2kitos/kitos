namespace Core.DomainModel
{
    public interface ILocaleEntity<TOriginal>
    {
        int MunicipalityId { get; set; }
        int OriginalId { get; set; }
        string Name { get; set; }

        Organization Organization { get; set; }
        TOriginal Original { get; set; }
    }
}