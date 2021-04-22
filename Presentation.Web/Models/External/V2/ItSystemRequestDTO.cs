using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2
{
    public class ItSystemRequestDTO
    {
        /// <summary>
        /// UUID for owning organization
        /// </summary>
        /// <remarks>Use organization API for getting a list of possible organizations related to the logged in user</remarks>
        [Required]
        public Guid RightsHolderUuid { get; set; }  

        /// <summary>
        /// UUID for IT-System
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }

        /// <summary>
        /// UUID for possible IT-System parent (if any)
        /// </summary>
        public Guid? ParentUuid { get; set; }

        /// <summary>
        /// Name of IT-System
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Former name of IT-System (if any)
        /// </summary>
        public string? FormerName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        [Required]
        public string UrlReference { get; set; }

        /// <summary>
        /// UUID for IT-System business type
        /// </summary>
        public Guid? BusinessTypeUuid { get; set; }

        /// <summary>
        /// KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<string>? KLENumbers { get; set; }

        /// <summary>
        /// UUID's for KLE numbers categorizing this IT-System
        /// </summary>
        public IEnumerable<Guid>? KLEUuids { get; set; }
    }
}