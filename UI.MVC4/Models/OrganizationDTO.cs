using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cvr { get; set; }

        public OrganizationType Type { get; set; }
        public AccessModifier AccessModifier { get; set; }
    }
}