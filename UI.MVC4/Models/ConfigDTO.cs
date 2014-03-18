
namespace UI.MVC4.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        /* IT SUPPORT */
        public int ItSupportModuleName_Id { get; set; }
        public string ItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
        public bool ShowColumnMandatory { get; set; }

        /* IT PROJECT */
        public int ItProjectModuleName_Id { get; set; }
        public string ItProjectGuide { get; set; }
        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }

        /* IT SYSTEM */
        public int ItSystemModuleName_Id { get; set; }
        public string ItSystemGuide { get; set; }

        /* IT CONTRACT */
        public int ItContractModuleName_Id { get; set; }
        public string ItContractGuide { get; set; }
    }
}