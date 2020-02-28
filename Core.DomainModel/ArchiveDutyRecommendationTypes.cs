namespace Core.DomainModel
{
    public enum ArchiveDutyRecommendationTypes
    {
        /// <summary>
        /// Covers the case where the choice is explicitly reset
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
        NoRecommendation = 3
    }
}
