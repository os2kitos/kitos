namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts
{
    public class InterfacesExposedOutsideTheOrganizationResponseDTO
    {
        public string ExposedInterfaceName { get; set; }
        public string ExposingSystemName { get; set; }
        public string OrganizationName { get; set; }
    }
}