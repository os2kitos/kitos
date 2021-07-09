namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    public class ExpectedUsersIntervalResponseDTO
    {
        /// <summary>
        /// Optional lower bound.
        /// If left unspecified, the interval has no lower bound.
        /// </summary>
        public int? LowerBound { get; set; }
        /// <summary>
        /// Optional upper bound
        /// If left unspecified the interval has no upper bound.
        /// </summary>
        public int? UpperBound { get; set; }
    }
}