using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.KLE
{
    public class KLEDetailsDTO
    {
        /// <summary>
        /// UUID of the KLE number
        /// </summary>
        [Required]
        public Guid Uuid { get; set; }
        /// <summary>
        /// KLE number from KLE-Online e.g. 00.01.10
        /// </summary>
        [Required]
        public string KleNumber { get; set; }
        /// <summary>
        /// KLE description from KLE-Online
        /// </summary>
        [Required]
        public string Description { get; set; }
        /// <summary>
        /// Optional parent KLE
        /// </summary>
        public IdentityNamePairResponseDTO ParentKle { get; set; }
    }
}