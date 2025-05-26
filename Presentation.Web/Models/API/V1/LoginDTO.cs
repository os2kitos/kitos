namespace Presentation.Web.Models.API.V1
{
    public class LoginDTO : UserCredentialsDTO
    {
        public bool RememberMe { get; set; }
    }


    public class OrganizationAndDefaultUnitDTO
    {
        public OrganizationDTO Organization { get; set; }
        public OrgUnitSimpleDTO DefaultOrgUnit { get; set; }
    }
}
