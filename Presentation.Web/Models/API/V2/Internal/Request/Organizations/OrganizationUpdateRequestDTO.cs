
namespace Presentation.Web.Models.API.V2.Internal.Request.Organizations
{
    public class OrganizationUpdateRequestDTO : OrganizationBaseRequestDTO
    {
        public bool UpdateForeignCountryCode { get; set; }
    }
}