namespace Core.DomainModel
{
    public enum ArchiveDutyTypes
    {
        /// <summary>
        /// Covers the case where the choice is explicitly reset
        /// </summary>
        Undecided = 0,
        /// <summary>
        /// B duty
        /// </summary>
        B = 1,
        /// <summary>
        /// K duty
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
        /// Unknown in the organization
        /// </summary>
        Unknown = 3,
        /// <summary>
        /// Data is kept, but selected or all documents are discarded
        /// </summary>
        PreserveDataCanDiscardDocuments = 4
    }
}
