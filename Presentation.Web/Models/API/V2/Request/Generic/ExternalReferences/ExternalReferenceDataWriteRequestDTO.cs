using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences
{
    /// <summary>
    /// User defined external references attached to a KITOS entity
    /// </summary>
    public class ExternalReferenceDataWriteRequestDTO
    {
        /// <summary>
        /// Reference title as shown in KITOS UI
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }
        /// <summary>
        /// Document ID/Case number
        /// </summary>
        public string DocumentId { get; set; }
        /// <summary>
        /// URL e.g. data sheet or other supplier related url.
        /// Also accepts ESDH system url's following the pattern: (kmdsageraabn|kmdedhvis|sbsyslauncher):.*
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Determines if this is the master reference for the KITOS entity
        /// Constraint: Only one external reference can be marked as the master reference
        /// </summary>
        [Required]
        public bool MasterReference { get; set; }
    }
}