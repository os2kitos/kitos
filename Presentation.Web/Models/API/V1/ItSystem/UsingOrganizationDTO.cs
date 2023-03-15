namespace Presentation.Web.Models.API.V1.ItSystem
{
    public class UsingOrganizationDTO
    {
        public NamedEntityWithUuidDTO SystemUsage{ get; set; }
        public NamedEntityWithUuidDTO Organization { get; set; }
    }
}