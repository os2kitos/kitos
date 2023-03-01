namespace Presentation.Web.Models.API.V2.Response.Shared
{
    public class ResourceCollectionPermissionsResponseDTO
    {
        /// <summary>
        /// True when API client is allowed to CREATE the resource
        /// </summary>
        public bool Create { get; set; }
    }
}