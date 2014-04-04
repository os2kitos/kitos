namespace Core.DomainModel.ItSystem
{
    public class Technology
    {
        public int Id { get; set; }
        public int DatabaseTypeId { get; set; }
        public int EnvironmentId { get; set; }
        public int ProgLanguageId { get; set; }

        public virtual DatabaseType DatabaseType { get; set; }
        public virtual Environment Environment { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual ProgLanguage ProgLanguage { get; set; }
    }
}
