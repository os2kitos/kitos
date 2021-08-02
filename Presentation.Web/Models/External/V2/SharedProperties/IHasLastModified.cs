using System;
using Presentation.Web.Models.External.V2.Response;

namespace Presentation.Web.Models.External.V2.SharedProperties
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
