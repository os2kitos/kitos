namespace Core.DomainModel
{
    public class Configuration : IEntity<int>
    {
        public int Id { get; set; }

        #region IT Support

        public string ItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnUsage { get; set; }
        public bool ShowColumnMandatory { get; set; }

        #endregion

        #region IT Project

        public string ItProjectGuide { get; set; }
        public bool ShowFocusArea { get; set; }
        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }
        
        #endregion

        #region IT System

        public string ItSystemGuide { get; set; }

        #endregion

        #region IT Contract
        
        public bool ShowRightOfUse { get; set; }
        public bool ShowLicense { get; set; }
        public bool ShowOperation { get; set; }
        public bool ShowMaintenance { get; set; }
        public bool ShowSupport { get; set; }
        public bool ShowServerLicense { get; set; }
        public bool ShowServerOperation { get; set; }
        public bool ShowBackup { get; set; }
        public bool ShowSurveillance { get; set; }
        public bool ShowOther { get; set; }

        #endregion

        public virtual Municipality Municipality { get; set; }
    }
}
