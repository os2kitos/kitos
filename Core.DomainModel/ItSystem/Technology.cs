namespace Core.DomainModel.ItSystem
{
    public partial class Technology
    {
        public int Id { get; set; }
        public int DatabaseType_Id { get; set; }
        public int Environment_Id { get; set; }
        public int ProgLanguage_Id { get; set; }

        public virtual DatabaseType DatabaseType { get; set; }
        public virtual Environment Environment { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual ProgLanguage ProgLanguage { get; set; }
    }
}
