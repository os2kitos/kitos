using System;

namespace Presentation.Web.Models.External.V2
{
    public class ItInterfaceResponseDTO
    {
        /// <summary>
        /// UUID for IT-Interface
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// UUID for IT-System exposing this IT-Interface
        /// </summary>
        public Guid ExposedBySystemUuid { get; set; }
        
        /// <summary>
        /// Name of IT-Interface
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Identifier for IT-Interface
        /// </summary>
        public string InterfaceId { get; set; }

        /// <summary>
        /// Version signifier for IT-Interface
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// General description of the IT-Interface
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        public string UrlReference { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        public bool Deactivated { get; set; }
        
        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Responsible for creation
        /// </summary>
        public IdentityNamePairResponseDTO CreatedBy { get; set; }

        /// <summary>
        /// Time of last modification
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Responsible for last modification
        /// </summary>
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
    }
}