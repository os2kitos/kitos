namespace UI.MVC4.Models
{
    public class ConfigDTO
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public string ItProjectName { get; set; }
        public bool ShowItSystemModule { get; set; }
        public string ItSystemName { get; set; }
        public bool ShowItContractModule { get; set; }
        public string ItContractName { get; set; }
    }
}