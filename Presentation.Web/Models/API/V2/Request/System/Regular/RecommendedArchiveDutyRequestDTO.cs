using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.System.Regular
{
    public class RecommendedArchiveDutyRequestDTO
    {
        /// <summary>
        /// Comment attached to the recommendation
        /// Constraints:
        /// - Only valid if 'Id' is NOT null or 'Undecided'
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Archive duty recommendation identifier
        /// </summary>
        public RecommendedArchiveDutyChoice? Id { get; set; }
    }
}