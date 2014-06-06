
namespace UI.MVC4.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        /* IT SUPPORT */
        public int ItSupportModuleNameId { get; set; }
        public string ItSupportModuleNameName { get; set; }
        public string ItSupportGuide { get; set; }
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
        public bool ShowColumnMandatory { get; set; }

        /* IT PROJECT */
        public int ItProjectModuleNameId { get; set; }
        public string ItProjectModuleNameName { get; set; }
        public string ItProjectGuide { get; set; }
        public bool ShowPortfolio { get; set; }
        public bool ShowBC { get; set; }

        /* IT SYSTEM */
        public int ItSystemModuleNameId { get; set; }
        public string ItSystemModuleNameName { get; set; }
        public string ItSystemGuide { get; set; }

        /* IT CONTRACT */
        public int ItContractModuleNameId { get; set; }
        public string ItContractModuleNameName { get; set; }
        public string ItContractGuide { get; set; }
    }
}