using Core.DomainModel.ItSystem;
using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Request
{
    public class RightsHolderWritableItInterfacePropertiesDTO
    {
        /// <summary>
        /// UUID for IT-System exposing this IT-Interface
        /// </summary>
        [Required] 
        public Guid ExposedBySystemUuid { get; set; }
        
        /// <summary>
        /// Name of IT-Interface
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        
        /// <summary>
        /// Identifier for IT-Interface
        /// </summary>
        public string InterfaceId { get; set; }
        
        /// <summary>
        /// Version signifier for IT-Interface
        /// </summary>
        [MaxLength(ItInterface.MaxVersionLength)]
        public string Version { get; set; }
        
        /// <summary>
        /// General description of the IT-Interface
        /// </summary>
        [Required]
        public string Description { get; set; }
        
        /// <summary>
        /// Url reference for further information
        /// </summary>
        [Required]
        public string UrlReference { get; set; }
    }
}