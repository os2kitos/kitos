using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.System.RightsHolder
{
    public class RightsHolderUpdateSystemPropertiesRequestDTO : IRightsHolderWritableSystemPropertiesRequestDTO
    {
        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        [NonEmptyGuid]
        public Guid? ParentUuid { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
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
        public string UrlReference { get; set; } //TODO: Replace with proper

        /// <summary>
        /// UUID for IT-System business type
        /// </summary>
        [NonEmptyGuid]
        public Guid? BusinessTypeUuid { get; set; }

        /// <summary>
        /// UUID's for KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}