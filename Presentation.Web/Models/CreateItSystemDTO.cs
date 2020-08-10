using Core.DomainModel;

namespace Presentation.Web.Models
{
    public class CreateItSystemDTO
    {
        public string Name { get; set; }
        public int OrganizationId { get; set; }
        public AccessModifier? AccessModifier { get; set; }
    }
}