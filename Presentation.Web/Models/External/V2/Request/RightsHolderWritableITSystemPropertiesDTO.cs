using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Request
{
    public class RightsHolderWritableITSystemPropertiesDTO
    {
        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
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
        public string UrlReference { get; set; }

        /// <summary>
        /// UUID for IT-System business type
        /// </summary>
        public Guid? BusinessTypeUuid { get; set; }

        /// <summary>
        /// KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<string> KLENumbers { get; set; }

        /// <summary>
        /// UUID's for KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<Guid> KLEUuids { get; set; }
    }
}