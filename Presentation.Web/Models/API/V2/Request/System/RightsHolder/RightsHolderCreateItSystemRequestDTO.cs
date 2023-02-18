using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.System.RightsHolder
{
    public class RightsHolderCreateItSystemRequestDTO : IRightsHolderWritableSystemPropertiesRequestDTO
    {
        /// <summary>
        /// UUID for owning organization
        /// </summary>
        /// <remarks>Use api/v2/rightsholder/organizations API for getting a list of possible organizations related to the logged in user</remarks>
        [Required]
        [NonEmptyGuid]
        public Guid RightsHolderUuid { get; set; }

        /// <summary>
        /// UUID for IT-System
        /// If no uuid is provided, KITOS will assign one automatically
        /// </summary>
        [NonEmptyGuid]
        public Guid? Uuid { get; set; }
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
        [Required(AllowEmptyStrings = false)]
        public string Description { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        [Required(AllowEmptyStrings = false)]
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
    }
}