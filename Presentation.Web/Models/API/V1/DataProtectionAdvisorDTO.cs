namespace Presentation.Web.Models.API.V1
{
    public class DataProtectionAdvisorDTO
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Cvr { get; set; }
        public string Phone { get; set; }
        public string Adress { get; set; }
        public string Email { get; set; }
        public int? OrganizationId { get; set; }
    }
}
