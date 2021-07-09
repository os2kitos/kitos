namespace Presentation.Web.Models.External.V2.Types.Shared
{
    public enum RecommendedArchiveDuty
    {
        /// <summary>
        /// Registration has been explicitly reset to "blank" by a user.
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