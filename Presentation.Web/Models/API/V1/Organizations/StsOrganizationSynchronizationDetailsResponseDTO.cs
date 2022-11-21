using System;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationSynchronizationDetailsResponseDTO
    {
        public StsOrganizationAccessStatusResponseDTO AccessStatus { get; set; }
        public bool Connected { get; set; }
        public bool SubscribesToUpdates { get; set; }
        public DateTime? DateOfLatestCheckBySubscription { get; set; }
        public int? SynchronizationDepth { get; set; }
        public bool CanCreateConnection { get; set; }
        public bool CanUpdateConnection { get; set; }
        public bool CanDeleteConnection { get; set; }
    }
}