using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Response.Organizations
{
    public class ConnectToStsOrganizationRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int? SynchronizationDepth { get; set; }
        public bool? SubscribeToUpdates { get; set; }
    }
}