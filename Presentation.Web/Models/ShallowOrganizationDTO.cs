namespace Presentation.Web.Models
{
    public class ShallowOrganizationDTO : NamedEntityDTO
    {
        public ShallowOrganizationDTO(int id, string name) : base(id, name)
        {
        }

        public string CvrNumber { get; set; }
    }
}