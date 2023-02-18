using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.System.Shared;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.System.Regular
{
    public class CreateItSystemRequestDTO : IItSystemWriteRequestCommonPropertiesDTO, IItSystemWriteRequestPropertiesDTO
    {
        /// <summary>
        /// UUID of the organization in which the it-system will be created
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid OrganizationUuid { get; set; }
        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        [NonEmptyGuid]
        public Guid? ParentUuid { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MaxLength(Core.DomainModel.ItSystem.ItSystem.MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// Former name of IT-System (if any)
        /// </summary>
        public string FormerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        public string UrlReference { get; set; } //TODO: Proper model

        /// <summary>
        /// UUID for IT-System business type
        /// </summary>
        [NonEmptyGuid]
        public Guid? BusinessTypeUuid { get; set; }

        /// <summary>
        /// KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<string> KLENumbers { get; set; } //TODO: kill

        /// <summary>
        /// UUID's for KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<Guid> KLEUuids { get; set; }
        /// <summary>
        /// Optional rightsholder organization reference uuid
        /// </summary>
        public Guid? RightsHolderUuid { get; set; }
        /// <summary>
        /// Scope (if not provided, it will default to "Global"
        /// </summary>
        public RegistrationScopeChoice? Scope { get; set; }
        public RecommendedArchiveDutyRequestDTO RecommendedArchiveDuty { get; set; }
    }
}