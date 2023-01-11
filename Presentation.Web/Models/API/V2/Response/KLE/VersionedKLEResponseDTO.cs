using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Response.KLE
{
    /// <summary>
    /// Wraps the KLE response payload with a reference to the data from api.kle-online.dk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VersionedKLEResponseDTO<T>
    {
        /// <summary>
        /// Defines the version of KLE (from api.kle-online.dk) which the content in 'payload' is based on.
        /// </summary>
        [Required]
        public DateTime ReferenceVersion { get; set; }
        /// <summary>
        /// Requested payload
        /// </summary>
        [Required]
        public T Payload { get; set; }

    }
}