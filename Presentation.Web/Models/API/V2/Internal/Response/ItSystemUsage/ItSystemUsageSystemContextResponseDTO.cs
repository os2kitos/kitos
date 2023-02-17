﻿using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageSystemContextResponseDTO : IdentityNamePairResponseDTO
    {
        [Required]
        public bool Deactivated { get; set; }

        protected ItSystemUsageSystemContextResponseDTO()
        {
        }

        public ItSystemUsageSystemContextResponseDTO(Guid uuid, string name, bool deactivated) 
            : base(uuid, name)
        {
            Deactivated = deactivated;
        }
    }
}