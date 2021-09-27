using System;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.Tracking
{
    public class TrackingEventResponseDTO
    {
        public DateTime OccurredAtUtc { get; set; }
        public Guid EntityUuid { get; set; }
        public TrackedEntityTypeChoice EntityType { get; set; }
    }
}