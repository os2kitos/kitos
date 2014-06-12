using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cvr { get; set; }

        public OrganizationType Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public ConfigDTO Config { get; set; }
    }
}