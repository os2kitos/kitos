namespace Presentation.Web.Models.API.V2.Response.Shared
{
    public class ResourcePermissionsResponseDTO
    {
        /// <summary>
        /// True when API client is allowed to READ the resource
        /// </summary>
        public bool Read { get; set; }
        /// <summary>
        /// True when API client is allowed to MODIFY the resource
        /// </summary>
        public bool Modify { get; set; }
        /// <summary>
        /// True when the API client is allowed to DELETE the 
        /// </summary>
        public bool Delete { get; set; }

    }
}