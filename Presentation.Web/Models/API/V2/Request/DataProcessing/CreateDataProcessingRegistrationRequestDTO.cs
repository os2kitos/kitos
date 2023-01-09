using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.GDPR;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class CreateDataProcessingRegistrationRequestDTO : DataProcessingRegistrationWriteRequestDTO, IHasNameExternal, IHasExternalReference<ExternalReferenceDataWriteRequestDTO>
    {
        /// <summary>
        /// UUID of the organization the data processing registration will be created in
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }

        /// <summary>
        /// Name of the registration
        /// Constraints:
        ///     - Max length: 200
        ///     - Name must be unique within the organization
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MaxLength(DataProcessingRegistrationConstraints.MaxNameLength)]
        public string Name { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public IEnumerable<ExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}