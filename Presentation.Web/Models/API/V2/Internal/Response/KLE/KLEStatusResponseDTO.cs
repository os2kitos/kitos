namespace Presentation.Web.Models.API.V2.Internal.Response.KLE
{
    public class KLEStatusResponseDTO
    {
        public bool UpToDate { get; set; }
        public required string Version { get; set; }
    }
}