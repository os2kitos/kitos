namespace Presentation.Web.Models.API.V1
{
    public class StakeholderDTO
    {
        public int Id { get; set; }
        public int ItProjectId { get; set; }

        public string Name { get; set; }
        public string Role { get; set; }
        public string Downsides { get; set; }
        public string Benefits { get; set; }
        public int Significance { get; set; }
        public string HowToHandle { get; set; }
    }
}
