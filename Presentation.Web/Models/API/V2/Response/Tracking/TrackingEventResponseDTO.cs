using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.Tracking
{
    public class TrackingEventResponseDTO
    {
        [Required]
        public DateTime OccurredAtUtc { get; set; }
        [Required]
        public Guid EntityUuid { get; set; }
        [Required]
        public TrackedEntityTypeChoice EntityType { get; set; }
    }
}