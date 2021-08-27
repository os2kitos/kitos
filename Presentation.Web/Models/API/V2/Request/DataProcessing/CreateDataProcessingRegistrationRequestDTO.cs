using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class CreateDataProcessingRegistrationRequestDTO : DataProcessingRegistrationWriteRequestDTO
    {
        /// <summary>
        /// UUID of the organization the data processing registration will be created in
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }
    }
}