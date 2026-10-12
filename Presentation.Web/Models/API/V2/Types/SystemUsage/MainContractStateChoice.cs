namespace Presentation.Web.Models.API.V2.Types.SystemUsage
{
    /// <summary>
    /// Represents the state of the main contract for an IT system usage
    /// </summary>
    public enum MainContractStateChoice
    {
        /// <summary>
        /// No contract is associated with the system usage
        /// </summary>
        NoContract = 0,
        /// <summary>
        /// The associated contract is active
        /// </summary>
        Active = 1,
        /// <summary>
        /// The associated contract is inactive
        /// </summary>
        Inactive = 2
    }
}
