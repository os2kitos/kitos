
namespace UI.MVC4.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        /* ORGANIZATION */
        public bool ShowTabOverview { get; set; }
        public bool ShowColumnTechnology { get; set; }
        public bool ShowColumnUsage { get; set; }
    }
}