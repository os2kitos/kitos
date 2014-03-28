namespace Core.DomainModel
{
    public class Localization : IEntity<int>
    {
        public int Id { get; set; }

        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string IdmRef { get; set; }
        public string FolderRef { get; set; }

        #region IT Project

        public string ItProject { get; set; }
        public string ItProgram { get; set; }
        public string FocusArea { get; set; }
        public string Fase1 { get; set; }
        public string Fase2 { get; set; }
        public string Fase3 { get; set; }
        public string Fase4 { get; set; }
        public string Fase5 { get; set; }

        #endregion


        #region IT Contract

        public string RightOfUse { get; set; }
        public string License { get; set; }
        public string Operation { get; set; }
        public string Maintenance { get; set; }
        public string Support { get; set; }
        public string ServerLicense { get; set; }
        public string ServerOperation { get; set; }
        public string Backup { get; set; }
        public string Surveillance { get; set; }
        public string Other { get; set; }

        #endregion

        //public virtual Organization Organization { get; set; }
    }
}
