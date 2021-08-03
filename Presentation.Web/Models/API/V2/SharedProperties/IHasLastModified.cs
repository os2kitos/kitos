using System;
using Presentation.Web.Models.API.V2.Response;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasLastModified
    {
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
