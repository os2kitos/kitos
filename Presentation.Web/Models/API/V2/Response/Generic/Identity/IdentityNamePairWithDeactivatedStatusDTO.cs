using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.Generic.Identity
{
    public class IdentityNamePairWithDeactivatedStatusDTO : IdentityNamePairResponseDTO, IHasDeactivatedExternal
    {
        protected IdentityNamePairWithDeactivatedStatusDTO()
        {

        }
        public IdentityNamePairWithDeactivatedStatusDTO(Guid uuid, string name, bool deactivated)
        {
            Uuid = uuid;
            Name = name;
            Deactivated = deactivated;
        }

        /// <summary>
        /// Deactivated status of the entity
        /// </summary>
        [Required]
        public bool Deactivated { get; set; }
    }
}