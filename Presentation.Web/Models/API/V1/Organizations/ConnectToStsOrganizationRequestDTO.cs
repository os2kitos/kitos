using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ConnectToStsOrganizationRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int? SynchronizationDepth { get; set; }
        public bool? SubscribeToUpdates { get; set; }
    }
}