using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public class UpdateItInterfaceRequestDTO : IItInterfaceWritablePropertiesRequestDTO
    {
        /// <summary>
        /// Optional reference to exposing it-system resource
        /// </summary>
        [NonEmptyGuid]
        public Guid? ExposedBySystemUuid { get; set; }

        /// <summary>
        /// Determines if the it-interface has been disabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Scope (shared globally or only available for members of the owning org)
        /// </summary>
        public RegistrationScopeChoice Scope { get; set; }

        /// <summary>
        /// Optional reference to the interface type option
        /// </summary>
        [NonEmptyGuid]
        public Guid? ItInterfaceTypeUuid { get; set; }

        /// <summary>
        /// Optional interface data descriptions
        /// </summary>
        public IEnumerable<ItInterfaceDataRequestDTO> Data { get; set; }

        /// <summary>
        /// Optional internal note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Name of IT-Interface
        /// </summary>
        [MaxLength(ItInterface.MaxNameLength)]
        public string Name { get; set; }
        /// <summary>
        /// Identifier for IT-Interface
        /// </summary>
        [MaxLength(ItInterface.MaxNameLength)]
        public string InterfaceId { get; set; }

        /// <summary>
        /// Version signifier for IT-Interface
        /// </summary>
        [MaxLength(ItInterface.MaxVersionLength)]
        public string Version { get; set; }

        /// <summary>
        /// General description of the IT-Interface
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        public string UrlReference { get; set; }
    }
}