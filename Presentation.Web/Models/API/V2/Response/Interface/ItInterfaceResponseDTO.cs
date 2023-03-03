using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.Interface
{
    public class ItInterfaceResponseDTO : BaseItInterfaceResponseDTO, IHasLastModified
    {
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Responsible for last modification
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }

        [Required]
        public RegistrationScopeChoice Scope { get; set; }
        
        /// <summary>
        /// Cross reference to the interface-type used by the it-interface
        /// </summary>
        public IdentityNamePairResponseDTO ItInterfaceType { get; set; }
        
        /// <summary>
        /// Optional interface data descriptions
        /// </summary>
        [Required]
        public IEnumerable<ItInterfaceDataResponseDTO> Data { get; set; }
        /// <summary>
        /// Organization in which this it-interface master data was created
        /// </summary>
        [Required]
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
    }
}