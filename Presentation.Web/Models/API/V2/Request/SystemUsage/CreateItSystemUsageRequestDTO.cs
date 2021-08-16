using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class CreateItSystemUsageRequestDTO : BaseItSystemUsageWriteRequestDTO
    {
        [Required]
        [NonEmptyGuid]
        public Guid SystemUuid { get; set; }
        
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }

        public GeneralDataWriteRequestDTO General { get; set; }
    }
}