using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItProjectCatalogDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ItProjectTypeName { get; set; }
        public string ItProjectId { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public bool IsArchived { get; set; }
        public string ObjectOwnerName { get; set; }
        public string OrganizationName { get; set; }
    }
}