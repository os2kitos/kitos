namespace Presentation.Web.Models.API.V2.Internal.Request.Options
{
    public class LocalRoleOptionUpdateRequestDTO : LocalRegularOptionUpdateRequestDTO
    {
        public bool IsExternallyUsed { get; set; }
        public string? ExternallyUsedDescription { get; set; }
    }
}