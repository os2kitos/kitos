using System;

namespace Presentation.Web.Models.External.V2.Response.Interface
{
    public class ItInterfaceResponseDTO : BaseItInterfaceResponseDTO
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