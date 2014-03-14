namespace UI.MVC4.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }

        public bool ShowItProjectModule { get; set; }
        public int ItProjectNameId { get; set; }

        public bool ShowItSystemModule { get; set; }
        public int ItSystemNameId { get; set; }

        public bool ShowItContractModule { get; set; }
        public int ItContractNameId { get; set; }

        public bool ShowItSupportModule { get; set; }
        public int ItSupportNameId { get; set; }
    }
}