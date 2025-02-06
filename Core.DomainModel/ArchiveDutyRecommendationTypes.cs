namespace Core.DomainModel
{
    public enum ArchiveDutyRecommendationTypes
    {
        /// <summary>
        /// Covers the case where no registration has been entered yet or the field has been reset to "blank" by a user.
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
        /// No recommendation exists from the archiving authority
        /// </summary>
        NoRecommendation = 3,
        /// <summary>
        /// Recommends data is kept, selected or all documents are discarded
        /// </summary>
        PreserveDataCanDiscardDocuments = 4,
        
    }
}
