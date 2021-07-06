using System;

namespace Presentation.Web.Models.External.V2.Response.KLE
{
    public class KLEDetailsDTO
    {
        /// <summary>
        /// UUID of the KLE number
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// KLE number from KLE-Online e.g. 00.01.10
        /// </summary>
        public string KleNumber { get; set; }
        /// <summary>
        /// KLE description from KLE-Online
        /// </summary>
        public string Description { get; set; }
    }
}