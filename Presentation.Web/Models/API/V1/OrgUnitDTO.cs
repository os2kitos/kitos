namespace Presentation.Web.Models.API.V1
{
    public class OrgUnitSimpleDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int OrganizationId { get; set; }
        public required string OrganizationName { get; set; }

        public string QualifiedName => $"{Name}, {OrganizationName}";
    }
}
