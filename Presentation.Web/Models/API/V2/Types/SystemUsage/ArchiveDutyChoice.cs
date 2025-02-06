namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    public enum ArchiveDutyChoice
    {
        /// <summary>
        /// Registration has been explicitly reset
        /// </summary>
        Undecided = 0,
        /// <summary>
        /// B is recommended
        /// </summary>
        B = 1,
        /// <summary>
        /// K i recommended
        /// </summary>
        K = 2,
        /// <summary>
        /// Archive duty is unknown
        /// </summary>
        Unknown = 3,
        /// <summary>
        /// Data is kept, selected or all documents are discarded
        /// </summary>
        PreserveDataCanDiscardDocuments = 4
    }
}