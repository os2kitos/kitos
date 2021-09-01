using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Token { get; set; }
    }


    public class OrganizationAndDefaultUnitDTO
    {
        public OrganizationDTO Organization { get; set; }
        public OrgUnitSimpleDTO DefaultOrgUnit { get; set; }
    }

    public class LoginResponseDTO
    {
        public UserDTO User { get; set; }
        public IEnumerable<OrganizationAndDefaultUnitDTO> Organizations { get; set; }
    }
}
