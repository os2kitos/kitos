namespace Presentation.Web.Models.API.V2.Types.Shared
{
    /// <summary>
    /// User defined external references attached to a KITOS entity
    /// </summary>
    public class ExternalReferenceDataDTO
    {
        /// <summary>
        /// Reference title as shown in KITOS UI
        /// </summary>
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
        /// </summary>
        public bool MasterReference { get; set; }
    }
}