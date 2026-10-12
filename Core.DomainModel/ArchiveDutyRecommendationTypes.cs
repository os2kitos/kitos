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
        /// No recommendation exists from the archiving authority
        /// </summary>
        NoRecommendation = 3,
        /// <summary>
        /// Recommends data is kept, selected or all documents are discarded
        /// </summary>
        PreserveDataCanDiscardDocuments = 4,
    }
}
