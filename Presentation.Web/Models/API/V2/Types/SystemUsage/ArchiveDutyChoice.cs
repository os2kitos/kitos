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
        /// B+K duty
        /// </summary>
        BK = 5,
        /// <summary>
        /// K+D duty
        /// </summary>
        KD = 6,
        /// <summary>
        /// K+B duty
        /// </summary>
        KB = 7,
        /// <summary>
        /// D+K duty
        /// </summary>
        DK = 8,
        /// <summary>
        /// D+D duty
        /// </summary>
        DD = 9,
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