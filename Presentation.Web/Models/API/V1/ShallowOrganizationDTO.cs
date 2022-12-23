namespace Presentation.Web.Models.API.V1
{
    public class ShallowOrganizationDTO : NamedEntityDTO
    {
        public ShallowOrganizationDTO(int id, string name) : base(id, name)
        {

        }

        public ShallowOrganizationDTO()
        {

        }

        public string CvrNumber { get; set; }
    }
}