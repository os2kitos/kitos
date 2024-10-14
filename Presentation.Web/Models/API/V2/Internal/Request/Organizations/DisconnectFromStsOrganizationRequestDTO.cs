namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class DisconnectFromStsOrganizationRequestDTO
    {
        /// <summary>
        /// If set to true, KITOS will purge all unused external units while disconnecting from STS Organization
        /// </summary>
        public bool PurgeUnusedExternalUnits { get; set; }
    }
}