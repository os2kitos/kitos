
namespace Presentation.Web.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }
        public bool ShowDataProcessing { get; set; }

        /* SHOW/HIDE 'IT' PREFIX */
        public bool ShowItProjectPrefix { get; set; }
        public bool ShowItSystemPrefix { get; set; }
        public bool ShowItContractPrefix { get; set; }

        /* ORGANIZATION */
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
    }
}
