using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItSystemDTO
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int? ExposedById { get; set; }
        public int OrganizationId { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }

        public AccessModifier AccessModifier { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }
    }
}