using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.Generic.Identity
{
    public class IdentityNamePairWithDeactivatedStatusDTO : IHasNameExternal, IHasUuidExternal, IHasDeactivatedExternal
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
        /// UUID which is unique within collection of entities of the same type
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }

        /// <summary>
        /// Human readable name of the entity
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Deactivated status of the entity
        /// </summary>
        [Required]
        public bool Deactivated { get; set; }
    }
}