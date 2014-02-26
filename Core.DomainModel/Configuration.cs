namespace Core.DomainModel
{
    public class Configuration : IEntity<int>
    {
        #region IT Project

        public int Id { get; set; }
        public string ItProjectGuide { get; set; }
        public string EsdhRef { get; set; }
        public string CmdbRef { get; set; }
        public string FolderRef { get; set; }
        public string ItProject { get; set; }
        public string ItProgram { get; set; }
        public string FocusArea { get; set; }
        public bool ShowFocusArea { get; set; }
        public string Fase1 { get; set; }
        public string Fase2 { get; set; }
        public string Fase3 { get; set; }
        public string Fase4 { get; set; }
        public string Fase5 { get; set; }

        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }
        
        #endregion

        public virtual Municipality Municipality { get; set; }
    }
}
