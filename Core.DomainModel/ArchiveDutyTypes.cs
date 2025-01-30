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
        /// Unknown in the organization
        /// </summary>
        Unknown = 3,
        /// <summary>
        /// Data is kept, but selected or all documents are discarded
        /// </summary>
        PreserveDataCanDiscardDocuments = 4
    }
}
