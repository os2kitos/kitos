﻿using System;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public class RightsHolderWritableItInterfacePropertiesDTO : IRightsHolderWritableItInterfacePropertiesDTO, IHasNameExternal
    {
        /// <summary>
        /// UUID for IT-System exposing this IT-Interface
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid ExposedBySystemUuid { get; set; }
        
        /// <summary>
        /// Name of IT-Interface
        /// </summary>
        [Required(AllowEmptyStrings = false)]
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
        [Required]
        public string Description { get; set; }
        
        /// <summary>
        /// Url reference for further information
        /// </summary>
        [Required]
        public string UrlReference { get; set; }
    }
}