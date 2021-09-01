namespace Presentation.Web.Models.API.V1
{
    public class RiskDTO
    {
        public int Id { get; set; }

        public int ItProjectId { get; set; }

        public string Name { get; set; }
        public string Action { get; set; }

        public int Probability { get; set; }
        public int Consequence { get; set; }

        public int? ResponsibleUserId { get; set; }
        public UserDTO ResponsibleUser { get; set; }
    }
}
