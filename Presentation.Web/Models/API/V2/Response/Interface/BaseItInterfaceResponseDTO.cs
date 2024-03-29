﻿using Presentation.Web.Models.API.V2.SharedProperties;
using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Interface
{
    public abstract class BaseItInterfaceResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator
    {
        /// <summary>
        /// UUID for IT-Interface
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }

        /// <summary>
        /// IT-System exposing this IT-Interface
        /// </summary>
        public IdentityNamePairResponseDTO ExposedBySystem { get; set; }

        /// <summary>
        /// Name of IT-Interface
        /// </summary>
        [Required]
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
        /// Internal notes regarding the IT-System (usually written by Global Admin)
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Url reference for further information
        /// </summary>
        public string UrlReference { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        [Required]
        public bool Deactivated { get; set; }

        /// <summary>
        /// Date of creation. (on some legacy systems , this information is not available. If so, it will be null)
        /// </summary>
        [Required]
        public DateTime? Created { get; set; }

        /// <summary>
        /// Responsible for creation
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
    }
}