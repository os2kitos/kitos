namespace Core.DomainModel
{
    public class KendoColumnConfiguration : IHasId
    {
        public int Id { get; set; }
        public string PersistId { get; set; }
        public int Index { get; set; }

        public int KendoOrganizationalConfigurationId { get; set; }
        public virtual KendoOrganizationalConfiguration KendoOrganizationalConfiguration { get; set; }
    }
}
